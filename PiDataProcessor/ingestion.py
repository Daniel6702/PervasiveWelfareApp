import threading
import logging
import json
import sys
from typing import Callable
import time

import paho.mqtt.client as mqtt

'''MQTT Ingestion Script'''

# Configure Logging
logging.basicConfig(
    level=logging.INFO,
    format='%(asctime)s %(levelname)s %(message)s',
    handlers=[
        logging.FileHandler("logs/app.log"),
        logging.StreamHandler(sys.stdout)
    ]
)

logger = logging.getLogger(__name__)

# Global Data Handler
_data_handler: Callable[[dict], None] = None

def handle_data(data):
    """
    Passes the received data to the data handler in a separate thread.
    """
    if _data_handler:
        threading.Thread(target=_data_handler, args=(data,), daemon=True).start()
    else:
        logger.error("Data handler not set. Please call set_data_handler before starting ingestion.")
        raise RuntimeError("Data handler not set. Please call set_data_handler before starting ingestion.")

def set_data_handler(data_processor: Callable[[dict], None]):
    """
    Sets the global data handler to the provided callable.
    """
    global _data_handler
    _data_handler = data_processor
    logger.info("Data handler has been set.")

def on_connect(client, userdata, flags, rc):
    """
    Callback when the client connects to the MQTT broker.
    """
    if rc == 0:
        logger.info("Connected to MQTT Broker successfully.")
        client.subscribe(userdata['topic'])
        logger.info(f"Subscribed to topic: {userdata['topic']}")
    else:
        logger.error(f"Failed to connect to MQTT Broker. Return code {rc}")

def on_message(client, userdata, msg):
    """
    Callback when a message is received from the subscribed topic.
    """
    try:
        payload = msg.payload.decode('utf-8')
        logger.info(f"Received message on {msg.topic}: {payload}")
        data = json.loads(payload)
        handle_data(data)
    except json.JSONDecodeError as e:
        logger.error(f"Failed to decode JSON payload: {e}")
    except Exception as e:
        logger.error(f"Error processing message: {e}")

def start_ingestion(
    broker_host: str = 'localhost',
    broker_port: int = 1883,
    topic: str = 'PigPi-1/data',
    client_id: str = 'IngestionClient',
    username: str = None,
    password: str = None,
    keepalive: int = 60
):
    """
    Starts the MQTT ingestion process.
    """
    client = mqtt.Client(client_id=client_id, userdata={'topic': topic})

    if username and password:
        client.username_pw_set(username, password)
        logger.info("MQTT client configured with authentication.")

    client.on_connect = on_connect
    client.on_message = on_message

    try:
        client.connect(broker_host, broker_port, keepalive)
    except Exception as e:
        logger.error(f"Could not connect to MQTT Broker: {e}")
        sys.exit(1)

    # Start the MQTT client loop in a separate thread
    client.loop_start()
    logger.info("MQTT client loop started.")

    try:
        while True:
            time.sleep(1)  # Keep the main thread alive
    except KeyboardInterrupt:
        logger.info("Interrupted by user. Shutting down...")
    finally:
        client.loop_stop()
        client.disconnect()
        logger.info("MQTT client disconnected.")