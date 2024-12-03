using Google.Cloud.Firestore;

namespace WelfareMonitorApp.Models;

[FirestoreData]
public class LongTermAnalysis
{
   [FirestoreProperty(name: "id")] public string PigId { get; set; }
   [FirestoreProperty(name: "timestamp")] public string TimeStamp { get; set; }
   [FirestoreProperty(name: "avg_movement")] public string AvgMovement { get; set; }
   [FirestoreProperty(name: "transition_probs")] public string TransitionProbs { get; set; }
}