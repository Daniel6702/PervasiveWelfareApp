from EventSystem import event_system
import paho.mqtt.client as mqtt
import json
from Models.MovementData import MovementData
from collections import defaultdict
import time
import threading
from typing import List, Tuple
from config import *
from Models.WelfareMsg import WelfareMsg

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
            self.analyze_data()
            time.sleep(self.analysis_interval)
            
    def analyze_data(self):
        current_time = time.time()
        cutoff_time = current_time - self.aggregation_period

        with self.lock:
            for pig_id, data_list in self.data.items():
                # Filter data within the aggregation period
                recent_data = [md for ts, md in data_list if ts >= cutoff_time]

                # Detect abnormalities
                welfare_score, note  = self.detect_abnormalities(recent_data)

                # Report abnormalities
                self.report_behavior(pig_id, welfare_score, note)

    def detect_abnormalities(self, data_list: List[MovementData]) -> Tuple[float, str]:
        """
        Calculate the welfare score based on movement, standing, laying behaviors, and distance moved.
        Only consider data where the keeper is not present.
        The welfare score is a value between 0 and 1.
        Returns the welfare score and a descriptive note.
        """
        if not data_list:
            return 0.0, "No data available"

        # Filter out data where keeper is present
        data_no_keeper = [md for md in data_list if md.keeper_presence_object_detect == 0]

        if not data_no_keeper:
            return 1.0, "Keeper always present"

        # Initialize counters
        movement_count = 0
        standing_count = 0
        laying_count = 0

        movement_duration = 0
        standing_duration = 0
        laying_duration = 0

        total_distance = 0.0  # Total distance moved in meters

        # Assuming data is received every second
        for md in data_no_keeper:
            if md.calc_movement_rf == 3:  # Moving
                movement_count += 1
                movement_duration += 1
                total_distance += md.distance
            elif md.calc_movement_rf == 2:  # Standing
                standing_count += 1
                standing_duration += 1
            elif md.calc_movement_rf == 1:  # Laying
                laying_count += 1
                laying_duration += 1

        # Calculate metrics per hour
        period_hours = self.aggregation_period / 3600
        movement_freq = movement_count / period_hours
        standing_freq = standing_count / period_hours
        laying_freq = laying_count / period_hours

        movement_dur = movement_duration / period_hours
        standing_dur = standing_duration / period_hours
        laying_dur = laying_duration / period_hours

        distance_moved = total_distance / period_hours  # meters per hour

        # Normalize each metric between 0 and 1 based on thresholds
        def normalize(value, min_val, max_val):
            if value < min_val:
                return 0.0
            elif value > max_val:
                return 1.0
            else:
                return (value - min_val) / (max_val - min_val)

        norm_movement_freq = normalize(movement_freq, MIN_MOVEMENT_FREQUENCY, MAX_MOVEMENT_FREQUENCY)
        norm_standing_freq = normalize(standing_freq, MIN_STANDING_FREQUENCY, MAX_STANDING_FREQUENCY)
        norm_laying_freq = normalize(laying_freq, MIN_LAYING_FREQUENCY, MAX_LAYING_FREQUENCY)

        norm_movement_dur = normalize(movement_dur, MIN_MOVEMENT_DURATION, MAX_MOVEMENT_DURATION)
        norm_standing_dur = normalize(standing_dur, MIN_STANDING_DURATION, MAX_STANDING_DURATION)
        norm_laying_dur = normalize(laying_dur, MIN_LAYING_DURATION, MAX_LAYING_DURATION)

        norm_distance_moved = normalize(distance_moved, MIN_DISTANCE_MOVED, MAX_DISTANCE_MOVED)

        # Combine metrics into a welfare score using weighted averages
        welfare_score = (
            WEIGHT_MOVEMENT_FREQUENCY * norm_movement_freq +
            WEIGHT_STANDING_FREQUENCY * norm_standing_freq +
            WEIGHT_LAYING_FREQUENCY * norm_laying_freq +
            WEIGHT_MOVEMENT_DURATION * norm_movement_dur +
            WEIGHT_STANDING_DURATION * norm_standing_dur +
            WEIGHT_LAYING_DURATION * norm_laying_dur +
            WEIGHT_DISTANCE_MOVED * norm_distance_moved
        )

        # Ensure the welfare score is between 0 and 1
        welfare_score = max(0.0, min(1.0, welfare_score))

        # Determine the most significant metric and generate a note
        note = self.generate_note({
            'movement_freq': norm_movement_freq,
            'standing_freq': norm_standing_freq,
            'laying_freq': norm_laying_freq,
            'movement_dur': norm_movement_dur,
            'standing_dur': norm_standing_dur,
            'laying_dur': norm_laying_dur,
            'distance_moved': norm_distance_moved
        })

        return welfare_score, note

    def generate_note(self, normalized_metrics: dict) -> str:
        """
        Determine the most significant deviation in the metrics and generate a descriptive note.
        """
        max_severity = 0.0
        selected_note = "Normal behavior"

        for metric, norm_value in normalized_metrics.items():
            thresholds = NOTE_THRESHOLDS.get(metric, {})
            messages = thresholds.get('messages', {})
            severity = 0.0
            direction = None

            # Check for low deviation
            if 'low' in thresholds:
                if norm_value < thresholds['low']:
                    deviation = thresholds['low'] - norm_value
                    severity = deviation / thresholds['low']  # Normalize severity
                    direction = 'low'

            # Check for high deviation
            if 'high' in thresholds:
                if norm_value > thresholds['high']:
                    deviation = norm_value - thresholds['high']
                    current_severity = deviation / (1.0 - thresholds['high'])
                    if current_severity > severity:
                        severity = current_severity
                        direction = 'high'

            # Update the selected note if this metric has higher severity
            if severity > max_severity and direction in messages:
                max_severity = severity
                selected_note = messages[direction]

        return selected_note

    def report_behavior(self, pig_id: str, welfare_score: float, note: str):
        msg = WelfareMsg(id=pig_id, score=welfare_score, note=note)
        event_system.publish(msg, 'welfare_report')