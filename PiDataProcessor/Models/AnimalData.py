from dataclasses import dataclass, asdict
from Models.Status import Status

@dataclass
class AnimalData:
    animal_id: str
    timestamp: str
    status: Status
    notes: str
    image_url: str

    def to_dict(self) -> dict:
        data_dict = asdict(self)
        data_dict['status'] = self.status.value  
        return data_dict

