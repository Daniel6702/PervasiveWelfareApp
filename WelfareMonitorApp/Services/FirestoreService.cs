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

        public async Task<List<string>> GetImageUrlsAsync()
        {
            CollectionReference collectionRef = _firestoreDb.Collection("images");
            QuerySnapshot snapshot = await collectionRef.GetSnapshotAsync();
            List<string> imageUrls = new List<string>();

            foreach (DocumentSnapshot document in snapshot.Documents)
            {
                if (document.TryGetValue("image_url", out string imageUrl))
                {
                    imageUrls.Add(imageUrl);
                }
            }

            return imageUrls;
        }

        public async Task<List<byte[]>> DownloadImagesAsync(List<string> imageUrls)
        {
            List<byte[]> imagesData = new List<byte[]>();
            using HttpClient httpClient = new HttpClient();

            foreach (string url in imageUrls)
            {
                byte[] imageData = await httpClient.GetByteArrayAsync(url);
                imagesData.Add(imageData);
            }

            return imagesData;
        }

        public async Task<List<byte[]>> GetImagesAsync()
        {
            List<string> imageUrls = await GetImageUrlsAsync();
            return await DownloadImagesAsync(imageUrls);
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



    }
}
