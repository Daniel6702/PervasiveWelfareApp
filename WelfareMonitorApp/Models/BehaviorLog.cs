namespace WelfareMonitorApp.Models
{
    public class BehaviorLog
    {
        public int Id { get; set; }
        public int PigId { get; set; }
        public DateTime Timestamp { get; set; }
        public string Behavior { get; set; }
        public string Notes { get; set; }
    }
}
