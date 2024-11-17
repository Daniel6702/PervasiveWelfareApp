// LoadingPage.xaml.cs
using Microsoft.Maui.Controls;
using WelfareMonitorApp.Helpers;
using WelfareMonitorApp.Services;

namespace WelfareMonitorApp.Views
{
    public partial class LoadingPage : ContentPage
    {
        private readonly IServiceProvider _serviceProvider;

        public LoadingPage()
        {
            InitializeComponent();

            // Access the service provider after it's initialized
            _serviceProvider = ServiceProviderAccessor.Instance;

            // Start the initialization
            InitializeApp();
        }

        private async void InitializeApp()
        {
            var isLoggedIn = await IsUserLoggedInAsync();

            if (isLoggedIn)
            {
                // User is logged in, navigate to AppShell
                App.Current.MainPage = new AppShell();
            }
            else
            {
                // User is not logged in, show LoginPage
                var loginPage = _serviceProvider.GetService(typeof(LoginPage)) as LoginPage;
                App.Current.MainPage = new NavigationPage(loginPage);
            }
        }

        private async Task<bool> IsUserLoggedInAsync()
        {
            try
            {
                var idToken = await SecureStorage.GetAsync("IdToken");
                var refreshToken = await SecureStorage.GetAsync("RefreshToken");
                var tokenExpiry = await SecureStorage.GetAsync("TokenExpiry");

                if (string.IsNullOrEmpty(idToken) || string.IsNullOrEmpty(refreshToken) || string.IsNullOrEmpty(tokenExpiry))
                    return false;

                var expiryDate = DateTime.Parse(tokenExpiry);
                if (DateTime.UtcNow > expiryDate)
                {
                    // Token has expired, attempt to refresh
                    var firebaseAuthService = _serviceProvider.GetService(typeof(FirebaseAuthService)) as FirebaseAuthService;
                    var newToken = await firebaseAuthService.RefreshAuthTokenAsync(refreshToken);

                    // Update stored tokens
                    await SecureStorage.SetAsync("IdToken", newToken.IdToken);
                    await SecureStorage.SetAsync("RefreshToken", newToken.RefreshToken);
                    await SecureStorage.SetAsync("TokenExpiry", DateTime.UtcNow.AddSeconds(int.Parse(newToken.ExpiresIn)).ToString());

                    // Update UserService with current user data
                    var userService = _serviceProvider.GetService(typeof(UserService)) as UserService;
                    var user = await firebaseAuthService.GetUserDataAsync(newToken.UserId);
                    userService.CurrentUser = user;

                    return true;
                }
                else
                {
                    // Token is valid, get user data
                    var firebaseAuthService = _serviceProvider.GetService(typeof(FirebaseAuthService)) as FirebaseAuthService;
                    var userService = _serviceProvider.GetService(typeof(UserService)) as UserService;

                    var userId = await SecureStorage.GetAsync("UserId");
                    userService.CurrentUser = await firebaseAuthService.GetUserDataAsync(userId);

                    return true;
                }
            }
            catch
            {
                // Any exception implies the user is not logged in
                return false;
            }
        }
    }
}
