from dataclasses import dataclass

@dataclass
class ImgMsg:
    img: bytes
    id: str
