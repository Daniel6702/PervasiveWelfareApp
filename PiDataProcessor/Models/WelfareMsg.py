from dataclasses import dataclass, asdict

@dataclass
class WelfareMsg:
    id: str
    score: float
    note: str

    def to_dict(self):
        return asdict(self)
    
    @classmethod
    def from_dict(cls, data: dict):
        return cls(**data)