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
    }
}
