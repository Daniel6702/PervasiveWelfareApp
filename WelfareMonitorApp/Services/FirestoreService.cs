// Services/FirestoreService.cs
using Google.Cloud.Firestore;
using Google.Apis.Auth.OAuth2;
using Google.Cloud.Firestore.V1;
using Grpc.Auth;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Maui.Storage;
using System;
using System.Collections.Generic;
using System.Net.Http;
using WelfareMonitorApp.Models;

namespace WelfareMonitorApp.Services
{
    public class FirestoreService
    {
        private readonly FirestoreDb _firestoreDb;

        private FirestoreService(FirestoreDb firestoreDb)
        {
            _firestoreDb = firestoreDb;
        }

        public static async Task<FirestoreService> CreateAsync(string projectId)
        {
            string jsonCredentials = await ReadCredentialsAsync();

            GoogleCredential credential = GoogleCredential.FromJson(jsonCredentials);

            FirestoreClientBuilder clientBuilder = new FirestoreClientBuilder
            {
                ChannelCredentials = credential.ToChannelCredentials()
            };

            FirestoreClient firestoreClient = await clientBuilder.BuildAsync();

            FirestoreDb firestoreDb = FirestoreDb.Create(projectId, firestoreClient);

            return new FirestoreService(firestoreDb);
        }
        private static async Task<string> ReadCredentialsAsync()
        {
            using var stream = await FileSystem.OpenAppPackageFileAsync("serviceAccountKey.json");
            using var reader = new StreamReader(stream);
            return await reader.ReadToEndAsync();
        }

        public async Task AddDataAsync(string collection, Dictionary<string, object> data)
        {
            CollectionReference collectionRef = _firestoreDb.Collection(collection);
            await collectionRef.AddAsync(data);
        }

        public async Task<List<DocumentSnapshot>> GetDataAsync(string collection)
        {
            CollectionReference collectionRef = _firestoreDb.Collection(collection);
            QuerySnapshot snapshot = await collectionRef.GetSnapshotAsync();
            return new List<DocumentSnapshot>(snapshot.Documents);
        }

        public async Task UpdateDataAsync(string collection, string document, Dictionary<string, object> data)
        {
            DocumentReference documentRef = _firestoreDb.Collection(collection).Document(document);
            await documentRef.SetAsync(data, SetOptions.MergeAll);
        }

        public async Task<List<PigImage>> GetPigImagesAsync()
        {
            CollectionReference collectionRef = _firestoreDb.Collection("images");
            QuerySnapshot snapshot = await collectionRef.GetSnapshotAsync();
            
            List<PigImage> pigImages = new List<PigImage>();

            foreach (DocumentSnapshot document in snapshot.Documents)
            {
                if (document.Exists)
                {
                    PigImage pigImage = document.ConvertTo<PigImage>();
                    pigImages.Add(pigImage);
                }
            }

            return pigImages;
        }

        public async Task<List<MovementData>> GetMovementDataAsync()
        {
            CollectionReference collectionRef = _firestoreDb.Collection("movement_data");
            QuerySnapshot snapshot = await collectionRef.GetSnapshotAsync();
            
            List<MovementData> movementDataList = new List<MovementData>();

            foreach (DocumentSnapshot document in snapshot.Documents)
            {
                if (document.Exists)
                {
                    MovementData movementData = document.ConvertTo<MovementData>();
                    movementDataList.Add(movementData);
                }
            }

            return movementDataList;
        }

        public async Task<List<WelfareLog>> GetWelfareDataAsync()
        {
            CollectionReference collectionRef = _firestoreDb.Collection("welfare_data");
            QuerySnapshot snapshot = await collectionRef.GetSnapshotAsync();
            
            List<WelfareLog> welfareLogList = new List<WelfareLog>();

            foreach (DocumentSnapshot document in snapshot.Documents)
            {
                if (document.Exists)
                {
                    WelfareLog welfareLog = document.ConvertTo<WelfareLog>();
                    welfareLogList.Add(welfareLog);
                }
            }

            return welfareLogList;
        }

        public async Task<List<LongTermAnalysis>> GetLongTermAnalysisAsync()
        {
            CollectionReference collectionRef = _firestoreDb.Collection("long_term_analysis");
            QuerySnapshot snapshot = await collectionRef.GetSnapshotAsync();

            List<LongTermAnalysis> longTermAnalysisList = new List<LongTermAnalysis>();

            foreach (DocumentSnapshot document in snapshot.Documents)
            {
                if (document.Exists)
                {
                    LongTermAnalysis longTermAnalysis = document.ConvertTo<LongTermAnalysis>();
                    longTermAnalysisList.Add(longTermAnalysis);
                }
            }

            return longTermAnalysisList;
        }
    }
}
