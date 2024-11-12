namespace WelfareMonitorApp.Models
{
    public class CurrentBehavoir
    {
        public string PigId { get; set; } //ID of the pig

        public string Behavior { get; set; } //laying, standing, moving

        public double Distance { get; set; } //distance moved in meters

        public string LastWalking { get; set; } //Number of frames since the pig was last walking

        public string PigClassObjectDetect { get; set; } //true = laying, false = standing

        public bool KeeperPresenceObjectDetect { get; set; } //true = keeper present, false = none
    }
}
