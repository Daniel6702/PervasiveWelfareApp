namespace WelfareMonitorApp.Models
{
    public enum RoleType
    {
        Researcher,
        AnimalCareTaker,
        Veterinarian,
        Admin,
        Observer
    }

    public class User
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public RoleType Role { get; set; }
        public string Email { get; set; }
        public string Password { get; set; } // Consider using secure methods for handling passwords.
    }
}
