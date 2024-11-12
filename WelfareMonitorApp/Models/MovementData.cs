using Google.Cloud.Firestore;

namespace WelfareMonitorApp.Models
{
    [FirestoreData]
    public class MovementData
    {
        [FirestoreProperty("timestamp")]
        public DateTimeOffset Timestamp { get; set; }

        [FirestoreProperty("calc_movement_rf")]
        public double CalcMovementRF { get; set; }  // RF model class (1 = laying, 2 = standing, 3 = moving)

        [FirestoreProperty("calc_movement_m2")]
        public double CalcMovementM2 { get; set; }  // New M2 version of activity

        [FirestoreProperty("calc_movement_m1")]
        public double CalcMovementM1 { get; set; }  // Lars and Frederik's original YOLO + Optical flow class

        [FirestoreProperty("m1")]
        public double M1 { get; set; }  // n+1 last Calc_movement_RF class, 0 = unknown, 1, 2, 3)

        [FirestoreProperty("m2")]
        public double M2 { get; set; }  // n+2

        [FirestoreProperty("m3")]
        public double M3 { get; set; }  // n+3

        [FirestoreProperty("distance")]
        public double Distance { get; set; }  // Distance moved - average calculated in M1

        [FirestoreProperty("rv")]
        public double RV { get; set; }  // Resulting vector - of last X, Y (n+1) and new X, Y (n)

        [FirestoreProperty("rv2")]
        public double RV2 { get; set; }  // last resulting vector (n+1)

        [FirestoreProperty("last_walking")]
        public int LastWalking { get; set; }  // Number of frames since the pig was last walking

        [FirestoreProperty("pig_class_object_detect")]
        public int PigClassObjectDetect { get; set; }  // YOLO object detect 1 = laying, 2 = standing

        [FirestoreProperty("pig_conf")]
        public double PigConf { get; set; }  // Confidence level (0-1) of the last object detect

        [FirestoreProperty("keeper_presence_object_detect")]
        public int KeeperPresenceObjectDetect { get; set; }  // YOLO object detect of keeper, 0 = none, 1 = keeper present

        [FirestoreProperty("keeper_conf")]
        public double KeeperConf { get; set; }  // Confidence level (0-1) of the last keeper detect

        [FirestoreProperty("center_x")]
        public double CenterX { get; set; }  // Center of the box, X

        [FirestoreProperty("center_y")]
        public double CenterY { get; set; }  // Center of the box, Y

        [FirestoreProperty("rf_class")]
        public int RFClass { get; set; }  // The Random forest class - 1, 2, 3 - could be different from CalcMovement_RF but likely is not

        [FirestoreProperty("rf_conf")]
        public double RFConf { get; set; }  // Confidence of the Random Forest (0-1)

        [FirestoreProperty("agreement")]
        public int Agreement { get; set; }  // To what extent the RF and M2 models agree

        [FirestoreProperty("pig_id")]
        public string PigId { get; set; } = null;
    }
}
