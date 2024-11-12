import time
import random
import json
import paho.mqtt.client as mqtt
from datetime import datetime
import threading
import os
from Models.MovementData import MovementData

class DataSimulator:
    def __init__(self, raspberry_pi_name):
        self.raspberry_pi_name = raspberry_pi_name
        self.last_timestamp = datetime.now()
        # Initialize any other state variables needed

    def generate_data(self):
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
            )
        data = movement_data.to_dict()
        return data

    def send_image(self, client, picture_topic):
        # Randomly select an image from the list
        image_filename = random.choice(['images/pig1.png', 'images/pig3.png', 'images/pig2.png'])
        try:
            with open(image_filename, 'rb') as image_file:
                image_data = image_file.read()
            client.publish(picture_topic, payload=image_data)
            print(f"[{self.raspberry_pi_name}] Sent image '{image_filename}' to topic '{picture_topic}'")
        except FileNotFoundError:
            print(f"Image file '{image_filename}' not found.")

def simulate_pi(pi_name):
    # Set up MQTT client for this Pi
    broker_address = "assure.au-dev.dk"
    port = 1883
    data_topic = f"{pi_name}/data"
    picture_topic = f"{pi_name}/picture"
    username = "s1"
    password = "passwordfors1"
    
    client = mqtt.Client()
    
    if username and password:
        client.username_pw_set(username, password)
    
    client.connect(broker_address, port, 60)
    client.loop_start()
    
    # Create the data simulator for this Pi
    simulator = DataSimulator(pi_name)
        
    try:
        while True:
            # Generate and send data
            data = simulator.generate_data()
            json_data = json.dumps(data)
            client.publish(data_topic, json_data)
            print(f"[{pi_name}] Published data to topic '{data_topic}': {json_data}")
            
            # Check if it's time to send an image
            simulator.send_image(client, picture_topic)
            
            time.sleep(10)  # simulate different intervals
    except KeyboardInterrupt:
        pass
    finally:
        client.loop_stop()
        client.disconnect()

# List of Pi names to simulate
pi_names = ["SimPigPi-1", "SimPigPi-2"]

# Start a separate thread for each simulated Pi
threads = []

for pi_name in pi_names:
    thread = threading.Thread(target=simulate_pi, args=(pi_name,), daemon=True)
    thread.start()
    threads.append(thread)

# Keep the main thread alive to let child threads run
try:
    while True:
        time.sleep(1)
except KeyboardInterrupt:
    print("Simulation stopped.")
