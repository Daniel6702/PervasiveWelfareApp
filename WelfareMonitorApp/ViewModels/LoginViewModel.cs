//LoginViewModel.cs

using System;
using System.Threading.Tasks;
using WelfareMonitorApp.Services;
using Microsoft.Maui.Controls;
using WelfareMonitorApp.Views;

namespace WelfareMonitorApp.ViewModels
{
    public class LoginViewModel : BaseViewModel
    {
        private readonly FirebaseAuthService _authService;
        private readonly IServiceProvider _serviceProvider;


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

        public Command NavigateToRegisterCommand { get; }

        public LoginViewModel(FirebaseAuthService authService, IServiceProvider serviceProvider)
        {
            _authService = authService;
            _serviceProvider = serviceProvider;
            LoginCommand = new Command(async () => await LoginAsync());
            NavigateToRegisterCommand = new Command(async () => await NavigateToRegisterAsync());
            IsErrorVisible = false;
        }

        private async Task LoginAsync()
        {
            try
            {
                IsErrorVisible = false;
                ErrorMessage = string.Empty;

                if (string.IsNullOrEmpty(Email) || string.IsNullOrEmpty(Password))
                {
                    ErrorMessage = "Please enter both email and password.";
                    IsErrorVisible = true;
                    return;
                }

                var user = await _authService.SignInWithEmailPassword(Email, Password);

                var userService = _serviceProvider.GetService(typeof(UserService)) as UserService;
                userService.CurrentUser = user;

                App.Current.MainPage = new AppShell();
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Login failed: {ex.Message}";
                IsErrorVisible = true;
            }
        }

        private async Task NavigateToRegisterAsync()
        {
            var registrationPage = _serviceProvider.GetService(typeof(RegistrationPage)) as RegistrationPage;
            await Application.Current.MainPage.Navigation.PushAsync(registrationPage);
        }


    }
}
