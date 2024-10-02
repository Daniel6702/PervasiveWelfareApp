from dataclasses import dataclass

@dataclass
class Message:
    title: str
    body: str
    data: dict

    def to_fcm_json(self, device_token: str) -> dict:
        return {
            "message": {
                "token": device_token,
                "notification": {
                    "title": self.title,
                    "body": self.body
                },
                "data": self.data
            }
        }