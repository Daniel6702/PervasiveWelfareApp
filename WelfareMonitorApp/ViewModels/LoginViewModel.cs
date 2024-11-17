using System;
using System.Threading.Tasks;
using WelfareMonitorApp.Services;
using Microsoft.Maui.Controls;

namespace WelfareMonitorApp.ViewModels
{
    public class LoginViewModel : BaseViewModel
    {
        private readonly FirebaseAuthService _authService;

        private string _email;
        public string Email
        {
            get => _email;
            set => SetProperty(ref _email, value);
        }

        private string _password;
        public string Password
        {
            get => _password;
            set => SetProperty(ref _password, value);
        }

        private string _errorMessage;
        public string ErrorMessage
        {
            get => _errorMessage;
            set => SetProperty(ref _errorMessage, value);
        }

        private bool _isErrorVisible;
        public bool IsErrorVisible
        {
            get => _isErrorVisible;
            set => SetProperty(ref _isErrorVisible, value);
        }

        public Command LoginCommand { get; }

        public LoginViewModel(FirebaseAuthService authService)
        {
            _authService = authService;
            LoginCommand = new Command(async () => await LoginAsync());
            IsErrorVisible = false;
        }

        private async Task LoginAsync()
        {
            try
            {
                // Hide error message before each login attempt
                IsErrorVisible = false;
                ErrorMessage = string.Empty;

                if (string.IsNullOrEmpty(Email) || string.IsNullOrEmpty(Password))
                {
                    ErrorMessage = "Please enter both email and password.";
                    IsErrorVisible = true;
                    return;
                }

                var token = await _authService.SignInWithEmailPassword(Email, Password);

                // Successful login (you might want to navigate to a different page)
                await Application.Current.MainPage.DisplayAlert("Success", "Login successful!", "OK");

                // Clear fields after login
                Email = string.Empty;
                Password = string.Empty;
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Login failed: {ex.Message}";
                IsErrorVisible = true;
            }
        }
    }
}
