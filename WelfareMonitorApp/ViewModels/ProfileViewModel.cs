// ProfileViewModel.cs
using Microsoft.Maui.Controls;
using System.Threading.Tasks;
using WelfareMonitorApp.Services;
using System.Windows.Input;
using WelfareMonitorApp.Views;

namespace WelfareMonitorApp.ViewModels
{
    public class ProfileViewModel : BaseViewModel
    {
        private readonly FirebaseAuthService _authService;
        private readonly UserService _userService;
        private readonly IServiceProvider _serviceProvider;

        public ProfileViewModel(FirebaseAuthService authService, UserService userService, IServiceProvider serviceProvider)
        {
            _authService = authService;
            _userService = userService;
            _serviceProvider = serviceProvider;

            // Initialize commands
            LogoutCommand = new Command(async () => await LogoutAsync());
            UpdateProfileCommand = new Command(async () => await UpdateProfileAsync());
            ResetPasswordCommand = new Command(async () => await ResetPasswordAsync());
            DeleteAccountCommand = new Command(async () => await DeleteAccountAsync());

            // Initialize properties
            LoadUserData();
        }

        // Properties bound to UI
        private string _name;
        public string Name
        {
            get => _name;
            set => SetProperty(ref _name, value);
        }

        private string _email;
        public string Email
        {
            get => _email;
            set => SetProperty(ref _email, value);
        }

        private string _role;
        public string Role
        {
            get => _role;
            set => SetProperty(ref _role, value);
        }

        private string _newPassword;
        public string NewPassword
        {
            get => _newPassword;
            set => SetProperty(ref _newPassword, value);
        }

        private string _confirmNewPassword;
        public string ConfirmNewPassword
        {
            get => _confirmNewPassword;
            set => SetProperty(ref _confirmNewPassword, value);
        }

        // List of roles for the Picker
        public List<string> Roles { get; } = new List<string> { "Keeper", "Researcher", "Observer", "Admin" };

        // Commands
        public ICommand LogoutCommand { get; }
        public ICommand UpdateProfileCommand { get; }
        public ICommand ResetPasswordCommand { get; }
        public ICommand DeleteAccountCommand { get; }

        // Methods
        private void LoadUserData()
        {
            var currentUser = _userService.CurrentUser;
            Name = currentUser.name;
            Email = currentUser.email;
            Role = currentUser.role;
        }

        private async Task LogoutAsync()
        {
            await _authService.LogoutAsync();
            _userService.Logout();

            // Navigate to LoginPage
            var loginPage = _serviceProvider.GetService(typeof(LoginPage)) as LoginPage;
            App.Current.MainPage = new NavigationPage(loginPage);
        }

        private async Task UpdateProfileAsync()
        {
            try
            {
                // Validate input
                if (string.IsNullOrEmpty(Name) || string.IsNullOrEmpty(Email) || string.IsNullOrEmpty(Role))
                {
                    await Application.Current.MainPage.DisplayAlert("Error", "Please fill in all fields.", "OK");
                    return;
                }

                if (!string.IsNullOrEmpty(NewPassword))
                {
                    if (NewPassword != ConfirmNewPassword)
                    {
                        await Application.Current.MainPage.DisplayAlert("Error", "New passwords do not match.", "OK");
                        return;
                    }
                }

                // Update user profile
                await _authService.UpdateUserProfileAsync(_userService.CurrentUser, Name, Email, Role, NewPassword);

                // Update UserService
                _userService.CurrentUser.name = Name;
                _userService.CurrentUser.email = Email;
                _userService.CurrentUser.role = Role;

                // Clear password fields
                NewPassword = string.Empty;
                ConfirmNewPassword = string.Empty;

                await Application.Current.MainPage.DisplayAlert("Success", "Profile updated successfully.", "OK");
            }
            catch (Exception ex)
            {
                await Application.Current.MainPage.DisplayAlert("Error", $"Failed to update profile: {ex.Message}", "OK");
            }
        }

        private async Task ResetPasswordAsync()
        {
            try
            {
                // Send password reset email
                await _authService.SendPasswordResetEmailAsync(Email);
                await Application.Current.MainPage.DisplayAlert("Success", "Password reset email sent.", "OK");
            }
            catch (Exception ex)
            {
                await Application.Current.MainPage.DisplayAlert("Error", $"Failed to send password reset email: {ex.Message}", "OK");
            }
        }

        private async Task DeleteAccountAsync()
        {
            var confirm = await Application.Current.MainPage.DisplayAlert("Confirm", "Are you sure you want to delete your account? This action cannot be undone.", "Yes", "No");
            if (confirm)
            {
                try
                {
                    await _authService.DeleteAccountAsync();

                    // Clear user data
                    await _authService.LogoutAsync();
                    _userService.Logout();

                    // Navigate to LoginPage
                    var loginPage = _serviceProvider.GetService(typeof(LoginPage)) as LoginPage;
                    App.Current.MainPage = new NavigationPage(loginPage);

                    await Application.Current.MainPage.DisplayAlert("Success", "Account deleted successfully.", "OK");
                }
                catch (Exception ex)
                {
                    await Application.Current.MainPage.DisplayAlert("Error", $"Failed to delete account: {ex.Message}", "OK");
                }
            }
        }
    }
}
