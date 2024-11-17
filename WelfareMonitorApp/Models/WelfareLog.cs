using Google.Cloud.Firestore;

namespace WelfareMonitorApp.Models
{
    [FirestoreData]
    public class WelfareLog
    {
        [FirestoreProperty("id")]
        public string PigId { get; set; }

        [FirestoreProperty("score")]
        public float Score { get; set; }

        [FirestoreProperty("note")]
        public string Note { get; set; }
    }
}
