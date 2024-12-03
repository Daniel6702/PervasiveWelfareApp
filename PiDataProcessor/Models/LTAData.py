from dataclasses import dataclass, asdict
from typing import List


@dataclass
class LTAData:
    pig_id: str
    avg_movement: float
    #transition_probs: List[float]

    def to_dict(self):
        return asdict(self)

    @classmethod
    def from_dict(cls, data: dict):
        return cls(**data)