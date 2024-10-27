using System.Net.Http.Json;

public class FirebaseAuthService
{
    private readonly HttpClient _httpClient;
    private const string FirebaseApiKey = "AIzaSyDGbx9swIaXCogspRpv3q9j0w2rBIU3eb4";

    public FirebaseAuthService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<string> SignInWithEmailPassword(string email, string password)
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
            return result.IdToken;
        }

        throw new Exception("Firebase authentication failed.");
    }

    private class SignInResult
    {
        public string IdToken { get; set; }
    }
}