from dataclasses import dataclass, asdict
from typing import List


@dataclass
class LTAData:
    pig_id: str
    datapoints: int
    percentage_laying: float
    percentage_standing: float
    percentage_moving: float
    avg_distance: float
    total_distance: float
    avg_confidence: float
    keeper_present: bool

    def to_dict(self):
        return asdict(self)

    @classmethod
    def from_dict(cls, data: dict):
        return cls(**data)