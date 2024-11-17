import time
import random
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
    event_system.subscribe(firebase_service.upload_pig_welfare, "welfare_report")
    
    pig_ids = ['pig1', 'pig2', 'pig3']
    
    for pig_id in pig_ids:
        msg = WelfareMsg(id=pig_id, score=random.uniform(0, 1), note="Normal behavior")
        event_system.publish(msg, 'welfare_report')
    
    print('Welfare data uploaded successfully!')

