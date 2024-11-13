from dataclasses import dataclass

@dataclass
class WelfareMsg:
    id: str
    score: float
    note: str