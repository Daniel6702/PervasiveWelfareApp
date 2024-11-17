// Services/TokenProvider.cs
using System.Threading.Tasks;

namespace WelfareMonitorApp.Services
{
    public class TokenProvider
    {
        private readonly IServiceProvider _serviceProvider;

        public TokenProvider(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public async Task<string> GetValidTokenAsync()
        {
            var idToken = await SecureStorage.GetAsync("IdToken");
            var refreshToken = await SecureStorage.GetAsync("RefreshToken");
            var tokenExpiry = await SecureStorage.GetAsync("TokenExpiry");

            var expiryDate = DateTime.Parse(tokenExpiry);
            if (DateTime.UtcNow > expiryDate)
            {
                // Token has expired, refresh it
                var firebaseAuthService = _serviceProvider.GetService(typeof(FirebaseAuthService)) as FirebaseAuthService;
                var newToken = await firebaseAuthService.RefreshAuthTokenAsync(refreshToken);

                // Update stored tokens
                await SecureStorage.SetAsync("IdToken", newToken.IdToken);
                await SecureStorage.SetAsync("RefreshToken", newToken.RefreshToken);
                await SecureStorage.SetAsync("TokenExpiry", DateTime.UtcNow.AddSeconds(int.Parse(newToken.ExpiresIn)).ToString());

                return newToken.IdToken;
            }
            else
            {
                return idToken;
            }
        }
    }
}
