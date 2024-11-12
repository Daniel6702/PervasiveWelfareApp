from Services.DataRecieverService import PigDataReceiver
from time import sleep
from EventSystem import event_system
from Services.DataRecieverService import RecieverManager
from Services.LiveDataService import LiveDataModule
from Services.LongTermAnalysisModule import LongTermAnalysisModule
from Services.BehavoirAnalysisService import CurrentBehaviorModule
from Services.FirebaseService import FirebaseService
from Services.NotificationService import NotificationService

receiver_manager = RecieverManager(event_type = 'message_received')

live_data_service = LiveDataModule()
event_system.subscribe(live_data_service.process_data, 'message_received')

while True:
    #Keep Alive
    sleep(1)