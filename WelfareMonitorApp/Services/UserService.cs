// Services/UserService.cs
namespace WelfareMonitorApp.Services
{
    public class UserService
    {
        public User CurrentUser { get; set; }

        public void Logout()
        {
            CurrentUser = null;
        }
    }
}
