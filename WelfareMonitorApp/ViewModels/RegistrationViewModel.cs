// RegistrationViewModel.cs
using System.Collections.Generic;
using System.Threading.Tasks;
using WelfareMonitorApp.Services;
using Microsoft.Maui.Controls;

namespace WelfareMonitorApp.ViewModels
{
    public class RegistrationViewModel : BaseViewModel
    {
        private readonly FirebaseAuthService _authService;

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

        private string _password;
        public string Password
        {
            get => _password;
            set => SetProperty(ref _password, value);
        }

        private string _confirmPassword;
        public string ConfirmPassword
        {
            get => _confirmPassword;
            set => SetProperty(ref _confirmPassword, value);
        }

        private string _selectedRole;
        public string SelectedRole
        {
            get => _selectedRole;
            set => SetProperty(ref _selectedRole, value);
        }

        private List<string> _roles = new List<string> { "Keeper", "Researcher", "Observer", "Admin" };
        public List<string> Roles
        {
            get => _roles;
            set => SetProperty(ref _roles, value);
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

        public Command RegisterCommand { get; }

        public RegistrationViewModel(FirebaseAuthService authService)
        {
            _authService = authService;
            RegisterCommand = new Command(async () => await RegisterAsync());
            IsErrorVisible = false;
        }

        private async Task RegisterAsync()
        {
            try
            {
                IsErrorVisible = false;
                ErrorMessage = string.Empty;

                if (string.IsNullOrEmpty(Name) || string.IsNullOrEmpty(Email) ||
                    string.IsNullOrEmpty(Password) || string.IsNullOrEmpty(ConfirmPassword) ||
                    string.IsNullOrEmpty(SelectedRole))
                {
                    ErrorMessage = "Please fill in all the fields.";
                    IsErrorVisible = true;
                    return;
                }

                if (Password != ConfirmPassword)
                {
                    ErrorMessage = "Passwords do not match.";
                    IsErrorVisible = true;
                    return;
                }

                var token = await _authService.SignUpWithEmailPassword(Name, Email, Password, SelectedRole);

                await Application.Current.MainPage.DisplayAlert("Success", "Registration successful!", "OK");

                // Clear fields after registration
                Name = string.Empty;
                Email = string.Empty;
                Password = string.Empty;
                ConfirmPassword = string.Empty;
                SelectedRole = null;

                // Navigate back to the login page
                await Application.Current.MainPage.Navigation.PopAsync();
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Registration failed: {ex.Message}";
                IsErrorVisible = true;
            }
        }
    }
}
