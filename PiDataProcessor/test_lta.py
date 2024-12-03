import time
import random

from Models.LTAData import LTAData
from Services.FirebaseService import FirebaseService
from config import *
from Models.WelfareMsg import WelfareMsg
from EventSystem import event_system

if __name__ == "__main__":
    firebase_service = FirebaseService(
        credentials_path=CREDENTIALS_PATH,
        storage_bucket=STORAGE_BUCKET,
        project_id=PROJECT_ID
    )
    event_system.subscribe(firebase_service.upload_lta, "long_term_analysis")

    pig_ids = ['pig1', 'pig2', 'pig3']

    for pig_id in pig_ids:
        msg = LTAData(pig_id=pig_id, avg_movement=random.uniform(0, 1), transition_probs=[random.uniform(0, 1), random.uniform(0, 1)])
        event_system.publish(msg, 'long_term_analysis')

    print('Welfare data uploaded successfully!')

