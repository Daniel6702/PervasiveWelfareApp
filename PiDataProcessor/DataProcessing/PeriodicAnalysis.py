from EventSystem import event_system
import paho.mqtt.client as mqtt
import json
from Models.MovementData import MovementData
from collections import defaultdict
import time
import threading

class PeriodicAnalysisModule:
    '''
    Aggregates data over a period of time and performs analysis at regular intervals.
    The resulting reports are sent to the app and the long term analysis module.
    '''
    def __init__(self, aggregation_period=24*3600, analysis_interval=3600):
        self.aggregation_period = aggregation_period  # in seconds, default 24 hours
        self.analysis_interval = analysis_interval  # in seconds, default 1 hour
        self.data = defaultdict(list)  # key: pig_id, value: list of (timestamp, MovementData)
        self.start_time = None
        self.lock = threading.Lock()

        # Start the analysis thread
        self.analysis_thread = threading.Thread(target=self.analysis_loop)
        self.analysis_thread.daemon = True  # Thread will exit when the main program exits
        self.analysis_thread.start()

    def process_data(self, msg: mqtt.MQTTMessage):
        id = msg.topic.split('/')[0]
        if msg.topic.endswith("data"):
            json_data = json.loads(msg.payload)
            movement_data = MovementData.from_dict(json_data)
            movement_data.pig_id = id

            # Set the start time if not already set
            if self.start_time is None:
                self.start_time = time.time() - movement_data.timestamp

            # Compute absolute timestamp
            absolute_timestamp = self.start_time + movement_data.timestamp

            # Add data to the pig's data list with thread safety
            with self.lock:
                self.data[id].append((absolute_timestamp, movement_data))

                # Remove old data beyond aggregation period
                cutoff_time = absolute_timestamp - self.aggregation_period
                self.data[id] = [(ts, md) for ts, md in self.data[id] if ts >= cutoff_time]

    def analysis_loop(self):
        while True:
            time.sleep(self.analysis_interval)
            self.analyze_data()

    def analyze_data(self):
        # Perform analysis with thread safety
        with self.lock:
            for pig_id, data_list in self.data.items():
                # Extract only the MovementData instances
                movement_data_list = [md for ts, md in data_list]
                # Detect abnormalities
                abnormalities = self.detect_abnormalities(movement_data_list)
                # Report abnormalities
                self.report_abnormalities(pig_id, abnormalities)

    def detect_abnormalities(self, data_list):
        # Implement the logic to detect abnormalities
        # For now, we'll return an empty list as a placeholder
        return []

    def report_abnormalities(self, pig_id, abnormalities):
        # Placeholder for reporting function
        # Send the reports to the app and the long term analysis module
        pass
