from EventSystem import event_system
import paho.mqtt.client as mqtt
import json
from Models.MovementData import MovementData
from Models.ImageMsg import ImgMsg
'''
Data will be used for three different purposes:
    - Live data: Images and current stats will be transfers directly to the cloud
    - Current behaviour: Data over from over a short period (last 10 minutes) will be analyzed and behaovior and well being will be determined
    - Long term behaviour / well being:
'''

class LiveDataModule:
    def __init__(self):
        pass

    def process_data(self, msg: mqtt.MQTTMessage):
        id = msg.topic.split('/')[0]
        if msg.topic.endswith("picture"):
            self.handle_image(msg.payload, id)
        elif msg.topic.endswith("data"):
            self.handle_data(msg.payload, id)

    def handle_image(self, image_bytes, id):
        event_system.publish(ImgMsg(image_bytes, id), 'update_live_image')

    def handle_data(self, data_bytes, id):
        try:
            json_data = json.loads(data_bytes)
            movement_data = MovementData.from_dict(json_data)
            movement_data.pig_id = id
            event_system.publish(movement_data, 'update_live_data')
        except Exception as e:
            print(f"Error processing data: {e}")