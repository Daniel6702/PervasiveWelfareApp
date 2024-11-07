from Services.DataRecieverService import PigDataReceiver
from Services.DataProcessingService import DataHandler
from time import sleep

handler = DataHandler()

receiver = PigDataReceiver(data_handler = handler.on_message)

while True:
    sleep(1)