// Services/UserService.cs
namespace WelfareMonitorApp.Services
{
    public class UserService
    {
        public User CurrentUser { get; set; }

        public bool IsGuestUser => CurrentUser != null && CurrentUser.role == "Guest";

        public void Logout()
        {
            CurrentUser = null;
        }
    }
}
