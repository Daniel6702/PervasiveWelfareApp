from Services.DataRecieverService import PigDataReceiver
from Services.DataProcessingService import DataHandler
from time import sleep
from EventSystem import event_system
from Services.DataRecieverService import RecieverManager

receiver_manager = RecieverManager()

handler = DataHandler()
event_system.subscribe('message_received', handler.on_message)

while True:
    sleep(1)