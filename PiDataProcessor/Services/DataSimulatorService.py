import time
import random
import json
import paho.mqtt.client as mqtt
from datetime import datetime
import threading
import os

class DataSimulator:
    def __init__(self, raspberry_pi_name):
        self.raspberry_pi_name = raspberry_pi_name
        self.last_timestamp = datetime.now()
        # Initialize any other state variables needed

    def generate_data(self):
        timestamp = datetime.now()
        CalcMovement_RF = random.choice([1, 2, 3, 4])
        CalcMovement_M2 = random.choice([1, 2, 3, 4])
        CalcMovement_M1 = random.choice([1, 2, 3, 4])
        M1 = round(random.uniform(0, 5), 2)
        M2 = round(random.uniform(0, 5), 2)
        M3 = round(random.uniform(0, 5), 2)
        Distance = round(random.uniform(0, 200), 2)
        RV = round(random.uniform(0, 100), 2)
        RV2 = round(random.uniform(0, 100), 2)
        LastWalking = str(random.randint(0, 1000))
        PigClassObjectDetect = random.choice(["Pig-laying", "Pig-standing", ""])
        PigConf = round(random.uniform(0, 1), 2)
        KeeperPresenceObjectDeteect = random.choice(["Keeper", ""])
        KeeperConf = round(random.uniform(0, 1), 2)
        CenterX = round(random.uniform(0, 640), 2)
        CenterY = round(random.uniform(0, 480), 2)
        RF_class = str(random.choice([1, 2, 3, 4]))
        RF_conf = round(random.uniform(0, 1), 2)
        Agreement = random.choice(["True", "False"])
        
        data = {
            "Timestamp": timestamp.isoformat(),
            "RaspberryPiName": self.raspberry_pi_name,
            "CalcMovement_RF": CalcMovement_RF,
            "CalcMovement_M2": CalcMovement_M2,
            "CalcMovement_M1": CalcMovement_M1,
            "M1": M1,
            "M2": M2,
            "M3": M3,
            "Distance": Distance,
            "RV": RV,
            "RV2": RV2,
            "LastWalking": LastWalking,
            "PigClassObjectDetect": PigClassObjectDetect,
            "PigConf": PigConf,
            "KeeperPresenceObjectDeteect": KeeperPresenceObjectDeteect,
            "KeeperConf": KeeperConf,
            "CenterX": CenterX,
            "CenterY": CenterY,
            "RF-class": RF_class,
            "RF-conf": RF_conf,
            "Agreement": Agreement
        }
        return data

    def send_image(self, client, picture_topic):
        # Randomly select an image from the list
        image_filename = random.choice(['images/1.png', 'images/2.png', 'images/3.png'])
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
    
    # Set up timing for image sending
    last_image_time = time.time()
    
    try:
        while True:
            current_time = time.time()
            # Generate and send data
            data = simulator.generate_data()
            json_data = json.dumps(data)
            client.publish(data_topic, json_data)
            print(f"[{pi_name}] Published data to topic '{data_topic}': {json_data}")
            
            # Check if it's time to send an image
            if current_time - last_image_time >= 10:
                simulator.send_image(client, picture_topic)
                last_image_time = current_time
            
            time.sleep(random.uniform(0.5, 1.5))  # simulate different intervals
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
    thread = threading.Thread(target=simulate_pi, args=(pi_name,))
    thread.start()
    threads.append(thread)

# Keep the main thread alive to let child threads run
try:
    while True:
        time.sleep(1)
except KeyboardInterrupt:
    print("Simulation stopped.")
