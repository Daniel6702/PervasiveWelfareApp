import json
from datetime import datetime
from collections import defaultdict
from threading import Timer
from typing import List

import numpy as np
import paho.mqtt.client as mqtt

from EventSystem import event_system
from Models.LTAData import LTAData
from Models.MovementData import MovementData


class LongTermAnalysisModule:
    def __init__(self, analysis_interval=60):
        event_system.subscribe(self.process_data, 'message_received')
        self.aggregated_data = defaultdict(list)
        self.analysis_interval = analysis_interval

    def process_data(self, msg: mqtt.MQTTMessage):
        pig_id = msg.topic.split('/')[0]
        if msg.topic.endswith("data"):
            json_data = json.loads(msg.payload)
            data = MovementData.from_dict(json_data)
            data.pig_id = pig_id

            self.aggregated_data[pig_id].append(data)

        if len(self.aggregated_data[pig_id]) > 1:  # Example: Process every 100 entries
            self.compute_metrics(pig_id)

    def compute_metrics(self, pig_id):
        data = self.aggregated_data[pig_id]
        states = [entry.calc_movement_rf for entry in data]

        # Calculate average movement
        avg_movement = np.mean([entry.distance for entry in data])

        # State transition probabilities
        #transition_counts = defaultdict(int)
        #for i in range(1, len(states)):
        #    transition_counts[(states[i - 1], states[i])] += 1
        #transition_probs = {k: v / sum(transition_counts.values()) for k, v in transition_counts.items()}

        # Publish insights
        self.publish(pig_id, float(avg_movement))

        # Clear data to avoid memory overload
        self.aggregated_data[pig_id] = []


    def publish(self, pig_id: str, avg_movement: float):
        """Publish computed insights to the output topic."""
        msg = LTAData(pig_id=pig_id, avg_movement=avg_movement)
        event_system.publish(msg, 'long_term_analysis')

    def schedule_periodic_analysis(self):
        """Schedule periodic analysis."""
        for pig_id in list(self.aggregated_data.keys()):
            self.compute_metrics(pig_id)
        Timer(self.analysis_interval, self.schedule_periodic_analysis).start()