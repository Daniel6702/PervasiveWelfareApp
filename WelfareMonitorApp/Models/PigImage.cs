using Google.Cloud.Firestore;

namespace WelfareMonitorApp.Models
{
    [FirestoreData]
    public class PigImage
    {
        [FirestoreProperty("pig_id")]
        public string PigId { get; set; }

        [FirestoreProperty("image_url")]
        public string ImageUrl { get; set; }
    }
}
