import json
from collections import defaultdict
from threading import Timer
import paho.mqtt.client as mqtt

from EventSystem import event_system
from Models.LTAData import LTAData
from Models.MovementData import MovementData


def publish(pig_id: str,
            datapoints: int,
            percentage_laying: float,
            percentage_standing: float,
            percentage_moving: float,
            avg_distance: float,
            total_distance: float,
            avg_confidence: float,
            keeper_present: bool):
    """Publish computed insights to the output topic."""
    msg = LTAData(
        pig_id=pig_id,
        datapoints=datapoints,
        percentage_laying=percentage_laying,
        percentage_standing=percentage_standing,
        percentage_moving=percentage_moving,
        avg_distance=avg_distance,
        total_distance=total_distance,
        avg_confidence=avg_confidence,
        keeper_present=keeper_present
    )
    event_system.publish(msg, 'long_term_analysis')


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

        if len(self.aggregated_data[pig_id]) >= 10:  # Set number of data point before processing
            self.compute_metrics(pig_id)

    def compute_metrics(self, pig_id):
        # Get the movement data for the pig given the pig id
        data = self.aggregated_data[pig_id]

        # Count the datapoints
        datapoints = len(data)

        # Percentage of each state over the given time frame
        states = [item.calc_movement_rf for item in data]
        percentage_laying = states.count(1) / len(states)
        percentage_standing = states.count(2) / len(states)
        percentage_moving = states.count(3) / len(states)

        # Distance moved
        distance = [item.distance for item in data]
        avg_distance = sum(distance) / len(distance)
        total_distance = sum(distance)

        # Average confidence of RF predictions
        confidence = [item.rf_conf for item in data]
        avg_confidence = sum(confidence) / len(confidence)

        # Was a keeper present in the time period?
        keeper = [item.keeper_presence_object_detect for item in data]
        keeper_present = any(keeper)

        # Publish insights
        publish(
            pig_id=pig_id,
            datapoints=datapoints,
            percentage_laying=percentage_laying,
            percentage_standing=percentage_standing,
            percentage_moving=percentage_moving,
            avg_distance=avg_distance,
            total_distance=total_distance,
            avg_confidence=avg_confidence,
            keeper_present=keeper_present
        )

        # Clear data to avoid memory overload
        self.aggregated_data[pig_id] = []

    def schedule_periodic_analysis(self):
        """Schedule periodic analysis."""
        for pig_id in list(self.aggregated_data.keys()):
            self.compute_metrics(pig_id)
        Timer(self.analysis_interval, self.schedule_periodic_analysis).start()