from Services.DataRecieverService import PigDataReceiver
from time import sleep
from EventSystem import event_system
from DataProcessing.LiveDataModule import LiveDataModule
from DataProcessing.LongTermAnalysis import LongTermAnalysisModule
from DataProcessing.PeriodicAnalysis import PeriodicAnalysisModule
from Services.FirebaseService import FirebaseService
from Services.NotificationService import NotificationService
from config import *

#Initialize services
firebase_service = FirebaseService(
        credentials_path=CREDENTIALS_PATH,
        storage_bucket=STORAGE_BUCKET,
        project_id=PROJECT_ID
    )

LTA = LongTermAnalysisModule(analysis_interval=60)

event_system.subscribe(firebase_service.upload_pig_data, 'update_live_data')
event_system.subscribe(firebase_service.upload_pig_image_msg, 'update_live_image')
event_system.subscribe(firebase_service.upload_lta, 'long_term_analysis')


#notification_service = NotificationService(CREDENTIALS_PATH)

#Initialize data recievers
#TOPICS = ['PigPi-1', 'PigPi-2']
TOPICS = ["SimPigPi-1", "SimPigPi-2"]

recievers = []
for topic in TOPICS:
    receiver = PigDataReceiver(topic, lambda msg: event_system.publish(msg, 'message_received'))
    recievers.append(receiver)
            
#Initialize data processing modules
live_data_service = LiveDataModule()
event_system.subscribe(live_data_service.process_data, 'message_received')

while True:
    sleep(1) #Keep Process Alive
