namespace WelfareMonitorApp.Models
{
    public class Pig
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Status { get; set; }
        public DateTime LastChecked { get; set; }
        public List<BehaviorLog> BehaviorLogs { get; set; }

        public string BehavioralSummary { get; set; }

        public string CurrentActivity { get; set; }

        public Pig()
        {
            BehaviorLogs = new List<BehaviorLog>();
        }
    }
}
