import paho.mqtt.client as mqtt

class MqttBroker:
    @staticmethod
    def start_client(
                    broker_address: str, 
                    port: int = 1883, 
                    on_connect: callable = lambda: print("Connected to MQTT Broker successfully."), 
                    on_message: callable = lambda: print("Received message on {msg.topic}: {payload}"),
                    username: str = None,
                    password: str = None):
        
        client = mqtt.Client()

        if username and password:
            client.username_pw_set(username, password)

        client.on_connect = on_connect
        client.on_message = on_message

        try:
            client.connect(broker_address, port, 60)
            client.loop_start() 
        except Exception as e:
            print(f"Failed to start MQTT client: {e}")

class PigDataReceiver:
    def __init__(self, 
                data_handler: callable,
                broker_address: str = 'assure.au-dev.dk', 
                port: int = 1883, 
                username: str = 's1',
                password: str = 'passwordfors1'):
        
        self.data_handler = data_handler
        MqttBroker().start_client(broker_address, port, self._on_connect, self._on_message, username, password)

    @staticmethod
    def _on_connect(client, userdata, flags, rc):
        if rc == 0:
            print("Connected to MQTT broker.")
            client.subscribe(f"{userdata}/picture")
            client.subscribe(f"{userdata}/data")
        else:
            print(f"Failed to connect, return code {rc}")

    def _on_message(self, client, userdata, msg: mqtt.MQTTMessage):
        self.data_handler(msg)