// FirebaseAuthService.cs
using System.Net.Http.Json;

namespace WelfareMonitorApp.Services
{
    public class FirebaseAuthService
    {
        private readonly HttpClient _httpClient;
        private const string FirebaseApiKey = "AIzaSyDGbx9swIaXCogspRpv3q9j0w2rBIU3eb4";
        private const string FirebaseDatabaseUrl = "https://pigwelfaremonitoring-default-rtdb.europe-west1.firebasedatabase.app/";

        public FirebaseAuthService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<User> SignInWithEmailPassword(string email, string password)
        {
            var signInRequest = new
            {
                email = email,
                password = password,
                returnSecureToken = true
            };

            var response = await _httpClient.PostAsJsonAsync($"https://identitytoolkit.googleapis.com/v1/accounts:signInWithPassword?key={FirebaseApiKey}", signInRequest);

            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<SignInResult>();
                var idToken = result.IdToken;
                var localId = result.LocalId;

                // Retrieve user data
                var user = await GetUserDataAsync(localId, idToken);
                return user;
            }

            throw new Exception("Firebase authentication failed.");
        }

        public async Task<string> SignUpWithEmailPassword(string name, string email, string password, string role)
        {
            var signUpRequest = new
            {
                email = email,
                password = password,
                returnSecureToken = true
            };

            var response = await _httpClient.PostAsJsonAsync($"https://identitytoolkit.googleapis.com/v1/accounts:signUp?key={FirebaseApiKey}", signUpRequest);

            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<SignUpResult>();
                var idToken = result.IdToken;
                var localId = result.LocalId;

                // Store additional user data
                await StoreUserDataAsync(localId, name, email, role, idToken);

                return idToken;
            }

            throw new Exception("Firebase registration failed.");
        }

        private async Task StoreUserDataAsync(string userId, string name, string email, string role, string idToken)
        {
            var userData = new
            {
                name = name,
                email = email,
                role = role
            };

            var response = await _httpClient.PutAsJsonAsync($"{FirebaseDatabaseUrl}users/{userId}.json?auth={idToken}", userData);

            if (!response.IsSuccessStatusCode)
            {
                throw new Exception("Failed to store user data.");
            }
        }

        private async Task<User> GetUserDataAsync(string userId, string idToken)
        {
            var response = await _httpClient.GetAsync($"{FirebaseDatabaseUrl}users/{userId}.json?auth={idToken}");

            if (response.IsSuccessStatusCode)
            {
                var userData = await response.Content.ReadFromJsonAsync<User>();
                return userData;
            }

            throw new Exception("Failed to retrieve user data.");
        }

        private class SignInResult
        {
            public string IdToken { get; set; }
            public string LocalId { get; set; }
        }

        private class SignUpResult
        {
            public string IdToken { get; set; }
            public string LocalId { get; set; }
        }
    }

    public class User
    {
        public string name { get; set; }
        public string email { get; set; }
        public string role { get; set; }
    }
}
