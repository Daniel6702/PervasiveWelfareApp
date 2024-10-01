namespace WelfareMonitorApp.Models
{
    public enum AlertLevel
    {
        Low,
        Medium,
        High
    }

    public class Alert
    {
        public int Id { get; set; }
        public int PigId { get; set; }
        public string Message { get; set; }
        public DateTime Timestamp { get; set; }
        public AlertLevel Level { get; set; }
    }
}
