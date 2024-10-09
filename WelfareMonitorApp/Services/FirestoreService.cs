// Services/FirestoreService.cs
using Google.Cloud.Firestore;
using Google.Apis.Auth.OAuth2;
using Google.Cloud.Firestore.V1;
using Grpc.Auth;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Maui.Storage;

namespace WelfareMonitorApp.Services
{
    public class FirestoreService
    {
        private readonly FirestoreDb _firestoreDb;

        public FirestoreService(string projectId)
        {
            _firestoreDb = InitializeFirestoreDbAsync(projectId).Result;
        }

        private async Task<FirestoreDb> InitializeFirestoreDbAsync(string projectId)
        {
            string jsonCredentials = await ReadCredentialsAsync();

            GoogleCredential credential = GoogleCredential.FromJson(jsonCredentials);

            // Create a FirestoreClient using the GoogleCredential
            FirestoreClientBuilder clientBuilder = new FirestoreClientBuilder
            {
                ChannelCredentials = credential.ToChannelCredentials()
            };

            FirestoreClient firestoreClient = clientBuilder.Build();

            // Pass the FirestoreClient and projectId to FirestoreDb.Create
            return FirestoreDb.Create(projectId, firestoreClient);
        }

        private async Task<string> ReadCredentialsAsync()
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
    }
}
