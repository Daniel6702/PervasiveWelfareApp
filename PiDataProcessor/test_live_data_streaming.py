import time
import random
from Services.FirebaseService import FirebaseService
from config import *
from Services.FirebaseService import MovementData

if __name__ == "__main__":
    firebase_service = FirebaseService(
        credentials_path=CREDENTIALS_PATH,
        storage_bucket=STORAGE_BUCKET,
        project_id=PROJECT_ID
    )
    
    pig_ids = ['pig1', 'pig2', 'pig3']
    
    while True:
        # Simulate updating movement data for each pig
        for pig_id in pig_ids:
            # Create a MovementData instance with random data
            movement_data = MovementData(
                timestamp=time.time(),
                calc_movement_rf=random.randint(1, 3),
                calc_movement_m2=random.randint(1, 3),
                calc_movement_m1=random.randint(1, 3),
                m1=random.randint(0, 3),
                m2=random.randint(0, 3),
                m3=random.randint(0, 3),
                distance=random.uniform(0, 10),
                rv=random.uniform(0, 5),
                rv2=random.uniform(0, 5),
                last_walking=random.randint(0, 100),
                pig_class_object_detect=random.randint(1, 2),
                pig_conf=random.uniform(0, 1),
                keeper_presence_object_detect=random.randint(0, 1),
                keeper_conf=random.uniform(0, 1),
                center_x=random.uniform(0, 640),
                center_y=random.uniform(0, 480),
                rf_class=random.randint(1, 3),
                rf_conf=random.uniform(0, 1),
                agreement=random.randint(0, 100),
                pig_id=pig_id
            )

            firebase_service.upload_pig_data(movement_data)

        print('Pig data uploaded successfully!')

        # Pause before next update
        time.sleep(10)
