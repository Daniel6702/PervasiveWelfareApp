from dataclasses import dataclass, asdict

@dataclass
class WelfareMsg:
    id: str
    score: float
    note: str

    def to_dict(self):
        return asdict(self)