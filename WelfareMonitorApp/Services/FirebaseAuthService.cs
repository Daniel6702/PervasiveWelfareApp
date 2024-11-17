// FirebaseAuthService.cs
using System.Net.Http.Json;

namespace WelfareMonitorApp.Services
{
    public class FirebaseAuthService
    {
        
        private readonly HttpClient _httpClient;

        private readonly IServiceProvider _serviceProvider;

        private const string FirebaseApiKey = "AIzaSyDGbx9swIaXCogspRpv3q9j0w2rBIU3eb4";
        private const string FirebaseDatabaseUrl = "https://pigwelfaremonitoring-default-rtdb.europe-west1.firebasedatabase.app/";

        public FirebaseAuthService(HttpClient httpClient, IServiceProvider serviceProvider)
        {
            _httpClient = httpClient;
            _serviceProvider = serviceProvider;
        }

        public async Task<SignInResult> SignInWithEmailPassword(string email, string password)
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
                return result;
            }

            var error = await response.Content.ReadAsStringAsync();
            throw new Exception($"Firebase authentication failed: {error}");
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

        public async Task<User> GetUserDataAsync(string userId)
        {
            var tokenProvider = _serviceProvider.GetService(typeof(TokenProvider)) as TokenProvider;
            var idToken = await tokenProvider.GetValidTokenAsync();

            var response = await _httpClient.GetAsync($"{FirebaseDatabaseUrl}users/{userId}.json?auth={idToken}");

            if (response.IsSuccessStatusCode)
            {
                var userData = await response.Content.ReadFromJsonAsync<User>();
                return userData;
            }

            throw new Exception("Failed to retrieve user data.");
        }

        public async Task<RefreshTokenResult> RefreshAuthTokenAsync(string refreshToken)
        {
            var refreshRequest = new
            {
                grant_type = "refresh_token",
                refresh_token = refreshToken
            };

            var response = await _httpClient.PostAsync($"https://securetoken.googleapis.com/v1/token?key={FirebaseApiKey}",
                new FormUrlEncodedContent(new Dictionary<string, string>
                {
                    { "grant_type", "refresh_token" },
                    { "refresh_token", refreshToken }
                }));

            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<RefreshTokenResult>();
                return result;
            }

            var error = await response.Content.ReadAsStringAsync();
            throw new Exception($"Token refresh failed: {error}");
        }

        public async Task UpdateUserProfileAsync(User currentUser, string newName, string newEmail, string newRole, string newPassword)
        {
            var idToken = await SecureStorage.GetAsync("IdToken");
            var userId = await SecureStorage.GetAsync("UserId");

            // Update authentication email and password if needed
            if (newEmail != currentUser.email || !string.IsNullOrEmpty(newPassword))
            {
                // Build request for updating email and/or password
                var updateAuthRequest = new Dictionary<string, object>
                {
                    { "idToken", idToken },
                    { "returnSecureToken", true }
                };

                if (newEmail != currentUser.email)
                    updateAuthRequest["email"] = newEmail;

                if (!string.IsNullOrEmpty(newPassword))
                    updateAuthRequest["password"] = newPassword;

                var authResponse = await _httpClient.PostAsJsonAsync($"https://identitytoolkit.googleapis.com/v1/accounts:update?key={FirebaseApiKey}", updateAuthRequest);

                if (authResponse.IsSuccessStatusCode)
                {
                    var authResult = await authResponse.Content.ReadFromJsonAsync<AuthUpdateResult>();

                    // Update tokens
                    await SecureStorage.SetAsync("IdToken", authResult.IdToken);
                    await SecureStorage.SetAsync("RefreshToken", authResult.RefreshToken);
                    await SecureStorage.SetAsync("TokenExpiry", DateTime.UtcNow.AddSeconds(int.Parse(authResult.ExpiresIn)).ToString());

                    // Update stored email if changed
                    if (newEmail != currentUser.email)
                    {
                        currentUser.email = newEmail;
                        await SecureStorage.SetAsync("UserId", authResult.LocalId);
                    }
                }
                else
                {
                    var error = await authResponse.Content.ReadAsStringAsync();
                    throw new Exception($"Failed to update authentication info: {error}");
                }
            }

            // Update user data in Realtime Database
            var updatedUserData = new
            {
                name = newName,
                email = newEmail,
                role = newRole
            };

            var dbResponse = await _httpClient.PutAsJsonAsync($"{FirebaseDatabaseUrl}users/{userId}.json?auth={idToken}", updatedUserData);

            if (!dbResponse.IsSuccessStatusCode)
            {
                var error = await dbResponse.Content.ReadAsStringAsync();
                throw new Exception($"Failed to update user data: {error}");
            }
        }

        public async Task SendPasswordResetEmailAsync(string email)
        {
            var resetRequest = new
            {
                requestType = "PASSWORD_RESET",
                email = email
            };

            var response = await _httpClient.PostAsJsonAsync($"https://identitytoolkit.googleapis.com/v1/accounts:sendOobCode?key={FirebaseApiKey}", resetRequest);

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                throw new Exception($"Failed to send password reset email: {error}");
            }
        }

        public async Task DeleteAccountAsync()
        {
            var idToken = await SecureStorage.GetAsync("IdToken");

            var deleteRequest = new
            {
                idToken = idToken
            };

            var response = await _httpClient.PostAsJsonAsync($"https://identitytoolkit.googleapis.com/v1/accounts:delete?key={FirebaseApiKey}", deleteRequest);

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                throw new Exception($"Failed to delete account: {error}");
            }

            // Remove user data from Realtime Database
            var userId = await SecureStorage.GetAsync("UserId");
            var dbResponse = await _httpClient.DeleteAsync($"{FirebaseDatabaseUrl}users/{userId}.json?auth={idToken}");

            if (!dbResponse.IsSuccessStatusCode)
            {
                var error = await dbResponse.Content.ReadAsStringAsync();
                throw new Exception($"Failed to delete user data: {error}");
            }
        }

        public async Task LogoutAsync()
        {
            // Clear tokens
            SecureStorage.Remove("IdToken");
            SecureStorage.Remove("RefreshToken");
            SecureStorage.Remove("TokenExpiry");
            SecureStorage.Remove("UserId");

            // Optionally, clear any cached data
            await Task.CompletedTask;
        }


        private class AuthUpdateResult
        {
            public string IdToken { get; set; }
            public string RefreshToken { get; set; }
            public string ExpiresIn { get; set; }
            public string Email { get; set; }
            public string LocalId { get; set; }
        }

        public class RefreshTokenResult
        {
            public string IdToken { get; set; }
            public string RefreshToken { get; set; }
            public string ExpiresIn { get; set; }
            public string TokenType { get; set; }
            public string UserId { get; set; }
            public string ProjectId { get; set; }
        }


        public class SignInResult
        {
            public string IdToken { get; set; }
            public string LocalId { get; set; }
            public string RefreshToken { get; set; }
            public string ExpiresIn { get; set; }
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
