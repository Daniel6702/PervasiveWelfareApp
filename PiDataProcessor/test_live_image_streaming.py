import time
import random
from Services.FirebaseService import FirebaseService
from Credentials.credentials import CREDENTIALS_PATH, STORAGE_BUCKET, PROJECT_ID

if __name__ == "__main__":
    firebase_service = FirebaseService(
        credentials_path=CREDENTIALS_PATH,
        storage_bucket=STORAGE_BUCKET,
        project_id=PROJECT_ID
    )
    
    pig_ids = ['pig1', 'pig2', 'pig3']
    image_paths = [f'images/{pig_id}.png' for pig_id in pig_ids]
    
    while True:
        # Shuffle pig_ids and image_paths to simulate different images for each pig
        random.shuffle(pig_ids)
        random.shuffle(image_paths)

        # Upload images with unique filenames
        for pig_id, image_path in zip(pig_ids, image_paths):
            firebase_service.upload_pig_image(pig_id, image_path)

        print('Images uploaded successfully!')

        # Pause before next upload cycle
        time.sleep(10)
