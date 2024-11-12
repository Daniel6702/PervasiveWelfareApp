from dataclasses import dataclass, asdict
import json

@dataclass
class MovementData:
    timestamp: float  # RELATIVE TO START TIME
    calc_movement_rf: float  # RF model class (1 = laying, 2 = standing, 3 = moving)
    calc_movement_m2: float  # New M2 version of activity
    calc_movement_m1: float  # Lars and Frederik's original YOLO + Optical flow class
    m1: int  # n+1 last Calc_movement_RF class, 0 = unknown, 1, 2, 3)
    m2: float  # n+2
    m3: float  # n+3
    distance: float  # Distance moved - average calculated in M1
    rv: float  # Resulting vector - of last X, Y (n+1) and new X, Y (n)
    rv2: float  # last resulting vector (n+1)
    last_walking: int  # Number of frames since the pig was last walking
    pig_class_object_detect: int  # YOLO object detect 1 = laying, 2 = standing
    pig_conf: float  # Confidence level (0-1) of the last object detect
    keeper_presence_object_detect: int  # YOLO object detect of keeper, 0 = none, 1 = keeper present
    keeper_conf: float  # Confidence level (0-1) of the last keeper detect
    center_x: float  # Center of the box, X
    center_y: float  # Center of the box, Y
    rf_class: int  # The Random forest class - 1, 2, 3 - could be different from CalcMovement_RF but likely is not
    rf_conf: float  # Confidence of the Random Forest (0-1)
    agreement: int  # To what extent the RF and M2 models agree
    pig_id: str = None  # Unique ID for the pig

    def to_dict(self):
        return asdict(self)
    
    def from_dict(self, data: dict):
        for key, value in data.items():
            setattr(self, key, value)

    def to_json(self):
        return json.dumps(self.to_dict())
    
    def from_json(self, json_str: str):
        self.from_dict(json.loads(json_str))
