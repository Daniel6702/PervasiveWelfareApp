from Models.AnimalData import AnimalData
from Models.Status import Status
from Models.Message import Message
from Services.FirebaseService import FirebaseService
import time
from Credentials.credentials import CREDENTIALS_PATH, STORAGE_BUCKET, PROJECT_ID

ANIMAL_ID = '1234' #ID for this specific PI / Animal

def time_and_date_to_string() -> str:
    return time.strftime('%Y%m%d%H%M%S')

if __name__ == "__main__":

    # Initialize Firebase Service
    firebase_service = FirebaseService(
        credentials_path=CREDENTIALS_PATH,
        storage_bucket=STORAGE_BUCKET,
        project_id=PROJECT_ID
    )

    # Upload image to Firebase Storage
    image_url = firebase_service.upload_image('sample_image.jpg', f'images/{ANIMAL_ID}/{time_and_date_to_string()}.jpg')

    # Create Sample AnimalData object
    animal_data = AnimalData(
        animal_id=ANIMAL_ID,
        timestamp=time_and_date_to_string(),
        status=Status.HIGH,
        notes='Low activity detected',
        image_url=image_url
    )

    # Upload AnimalData to Firestore
    firebase_service.upload_data('animal_data', animal_data.to_dict())

    # Send FCM message
    notification = Message(
        title='Alert',
        body='Low activity detected',
        data=animal_data.to_dict()
    )

    # Send FCM message to all devices
    firebase_service.send_message(notification)
