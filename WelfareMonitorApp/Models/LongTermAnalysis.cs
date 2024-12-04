using Google.Cloud.Firestore;

namespace WelfareMonitorApp.Models;

[FirestoreData]
public class LongTermAnalysis
{
   [FirestoreProperty(name: "pig_id")] public string PigId { get; set; }
   [FirestoreProperty(name: "datapoints")] public int dataPoints { get; set; }
   [FirestoreProperty(name: "percantage_laying")] public float percentageLaying { get; set; }
   [FirestoreProperty(name: "percentage_standing")] public float percentageStanding { get; set; }
   [FirestoreProperty(name: "percentage_moving")] public float percentageMoving { get; set; }
   [FirestoreProperty(name: "avg_distance")] public float avgDistance { get; set; }
   [FirestoreProperty(name: "total_distance")] public float totalDistance { get; set; }
   [FirestoreProperty(name: "avg_confidence")] public float avgConfidence { get; set; }
   [FirestoreProperty(name: "keeper_present")] public bool keeperPresent { get; set; }
}