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
from math import erf, sqrt

class PeriodicAnalysisModule:
    '''
    Aggregates data over a period of time and performs analysis at regular intervals.
    The resulting reports are sent to the app and the long term analysis module.
    Now uses a Gaussian model for each metric to calculate the probability and
    account for aggregation time.
    '''
    def __init__(self, aggregation_period=AGGREGATION_PERIOD, analysis_interval=ANALYSIS_INTERVAL):
        self.aggregation_period = aggregation_period  # in seconds, default 24 hours
        self.analysis_interval = analysis_interval    # in seconds, default 1 hour
        self.data = defaultdict(list)  # key: pig_id, value: list of (timestamp, MovementData)
        self.start_time = None
        self.lock = threading.Lock()

        # Start the analysis thread
        self.analysis_thread = threading.Thread(target=self.analysis_loop)
        self.analysis_thread.daemon = True  # Thread will exit when the main program exits
        self.analysis_thread.start()

    def process_data(self, msg: mqtt.MQTTMessage):
        pig_id = msg.topic.split('/')[0]
        if msg.topic.endswith("data"):
            json_data = json.loads(msg.payload)
            movement_data = MovementData.from_dict(json_data)
            movement_data.pig_id = pig_id

            # Set the start time if not already set
            if self.start_time is None:
                self.start_time = time.time() - movement_data.timestamp

            # Compute absolute timestamp
            absolute_timestamp = self.start_time + movement_data.timestamp

            # Add data to the pig's data list with thread safety
            with self.lock:
                self.data[pig_id].append((absolute_timestamp, movement_data))

                # Remove old data beyond aggregation period
                cutoff_time = absolute_timestamp - self.aggregation_period
                self.data[pig_id] = [(ts, md) for ts, md in self.data[pig_id] if ts >= cutoff_time]

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
                welfare_score, note = self.detect_abnormalities(recent_data)

                # Report abnormalities
                self.report_behavior(pig_id, welfare_score, note)

    def detect_abnormalities(self, data_list: List[MovementData]) -> Tuple[float, str]:
        """
        Calculate the welfare score based on a Gaussian model for each metric.
        The data is aggregated over self.aggregation_period (in seconds),
        which corresponds to period_hours of data.

        We compute probability of observed metrics given the normal distributions
        defined in config, and combine these probabilities into a single score.
        """

        if not data_list:
            return 0.0, "No data available"

        # Filter out data where keeper is present
        data_no_keeper = [md for md in data_list if md.keeper_presence_object_detect == 0]

        if not data_no_keeper:
            # If keeper always present, we cannot infer pig welfare from movement - assume normal.
            return 1.0, "Keeper always present"

        # Count occurrences and durations
        movement_count = 0
        standing_count = 0
        laying_count = 0

        movement_duration = 0
        standing_duration = 0
        laying_duration = 0

        total_distance = 0.0  # Total distance moved in meters

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

        # Convert aggregation period to hours
        period_hours = self.aggregation_period / 3600.0

        # Calculate per-hour metrics
        movement_freq = movement_count / period_hours
        standing_freq = standing_count / period_hours
        laying_freq = laying_count / period_hours

        movement_dur_per_hour = movement_duration / period_hours
        standing_dur_per_hour = standing_duration / period_hours
        laying_dur_per_hour = laying_duration / period_hours
        distance_moved_per_hour = total_distance / period_hours

        # Compute probabilities from Gaussian model
        # For a metric that is normally distributed per hour with mean=mu, std=sigma,
        # over 'period_hours' hours, the distribution of the total/average changes.
        # If we consider the aggregated metric as a sum of independent hourly observations:
        #   mean_total = mu * period_hours
        #   std_total = sigma * sqrt(period_hours)
        # For frequency/duration/distance (which we treat as averages or sums per hour),
        # we can directly consider them as "per hour" metrics and scale appropriately.

        # We'll assume these metrics represent an hourly average. If we consider them as sums,
        # then for a total sum: observed = metric * period_hours, mean_total = mu * period_hours,
        # std_total = sigma * sqrt(period_hours).
        # For simplicity, let's treat the given metric as an hourly average and transform
        # distributions accordingly.

        # Define helper functions for probability calculation
        def normal_cdf(z):
            # Normal CDF using the error function
            return 0.5 * (1 + erf(z / sqrt(2)))

        def metric_probability(observed, mean, std):
            # Given an hourly mean/std, for an observation over period_hours:
            # expected mean = mean (since we're using per-hour average)
            # expected std = std / sqrt(period_hours_of_sampling) if we had a sample mean,
            # but here the metric is essentially the per-hour average observed over the entire period.
            #
            # Actually, because we've computed these metrics as "counts/durations per hour" directly,
            # they are effectively a mean over the entire period. For a sample mean of n hours from a
            # normal dist N(mu, sigma²), the sample mean also follows N(mu, sigma²/n).
            # Here n = period_hours.
            adjusted_std = std / sqrt(period_hours)

            # If adjusted_std is 0 (degenerate), return 1 if observed == mean else 0
            if adjusted_std == 0:
                return 1.0 if abs(observed - mean) < 1e-9 else 0.0

            z = (observed - mean) / adjusted_std
            # We consider how likely it is to observe a value at least this extreme.
            # This is a two-tailed probability:
            p = 2.0 * (1.0 - normal_cdf(abs(z)))
            return max(min(p, 1.0), 0.0)

        # Compute probabilities for each metric using configured Gaussian params:
        # These params (means and stds) should be defined in config.py
        # Example (to be added to config.py):
        # MEAN_MOVEMENT_FREQUENCY = 2.0
        # STD_MOVEMENT_FREQUENCY = 0.5
        #
        # Add similar means/stds for other metrics as needed.

        movement_freq_p = metric_probability(movement_freq, MEAN_MOVEMENT_FREQUENCY, STD_MOVEMENT_FREQUENCY)
        standing_freq_p = metric_probability(standing_freq, MEAN_STANDING_FREQUENCY, STD_STANDING_FREQUENCY)
        laying_freq_p = metric_probability(laying_freq, MEAN_LAYING_FREQUENCY, STD_LAYING_FREQUENCY)
        movement_dur_p = metric_probability(movement_dur_per_hour, MEAN_MOVEMENT_DURATION, STD_MOVEMENT_DURATION)
        standing_dur_p = metric_probability(standing_dur_per_hour, MEAN_STANDING_DURATION, STD_STANDING_DURATION)
        laying_dur_p = metric_probability(laying_dur_per_hour, MEAN_LAYING_DURATION, STD_LAYING_DURATION)
        distance_moved_p = metric_probability(distance_moved_per_hour, MEAN_DISTANCE_MOVED, STD_DISTANCE_MOVED)

        # Combine metrics into a single welfare score.
        # The probabilities themselves are between 0 and 1, representing how likely (normal) the observation is.
        # Weighted average of these probabilities:
        welfare_score = (
            WEIGHT_MOVEMENT_FREQUENCY * movement_freq_p +
            WEIGHT_STANDING_FREQUENCY * standing_freq_p +
            WEIGHT_LAYING_FREQUENCY * laying_freq_p +
            WEIGHT_MOVEMENT_DURATION * movement_dur_p +
            WEIGHT_STANDING_DURATION * standing_dur_p +
            WEIGHT_LAYING_DURATION * laying_dur_p +
            WEIGHT_DISTANCE_MOVED * distance_moved_p
        )

        # Ensure the welfare score is between 0 and 1
        welfare_score = max(0.0, min(1.0, welfare_score))

        # Generate a note based on which metric is most abnormal
        # We'll adapt the generate_note function to now look at probabilities rather than normalized metrics.
        notes = {
            'movement_freq': movement_freq_p,
            'standing_freq': standing_freq_p,
            'laying_freq': laying_freq_p,
            'movement_dur': movement_dur_p,
            'standing_dur': standing_dur_p,
            'laying_dur': laying_dur_p,
            'distance_moved': distance_moved_p
        }

        note = self.generate_note_from_probs(notes)
        return welfare_score, note

    def generate_note_from_probs(self, probabilities: dict) -> str:
        """
        Determine the most significant abnormal metric by looking at probabilities.
        Low probability = more abnormal.

        We'll reuse NOTE_THRESHOLDS but interpret them differently:
        Now 'low' or 'high' thresholds in NOTE_THRESHOLDS could be interpreted
        in terms of probability (e.g., if probability < 0.3 = 'low', if > 0.7 = 'high'),
        but this depends on your definition. You may want to adjust NOTE_THRESHOLDS
        or their interpretation.

        For this example, we assume:
        - If probability < 'low': the observed value is significantly lower than expected (or abnormal on low side).
        - If probability < 'high' does not make sense for probability since probability is always ≤1,
          but we may interpret 'high' as probability > threshold means very normal (so no note needed).

        We'll focus mainly on 'low' thresholds to indicate abnormality.
        """
        max_severity = 0.0
        selected_note = "Normal behavior"

        for metric, prob_value in probabilities.items():
            thresholds = NOTE_THRESHOLDS.get(metric, {})
            messages = thresholds.get('messages', {})
            severity = 0.0
            direction = None

            # Interpreting thresholds: If probability < 'low' threshold, abnormal
            # If probability is 'high', it's very normal, so likely no abnormal note needed.
            # Adjust as per your domain logic.
            if 'low' in thresholds:
                if prob_value < thresholds['low']:
                    # The lower the probability, the more severe
                    # severity can be (threshold['low'] - prob_value) / threshold['low']
                    # but since 'low' is likely between 0 and 1:
                    deviation = thresholds['low'] - prob_value
                    severity = deviation / thresholds['low']
                    direction = 'low'

            # Check if there's a 'high' threshold:
            # If defined, maybe it means extremely normal? Typically not needed for abnormal note.
            # We'll ignore 'high' here since it doesn't make sense with probabilities.
            
            if severity > max_severity and direction in messages:
                max_severity = severity
                selected_note = messages[direction]

        return selected_note

    def report_behavior(self, pig_id: str, welfare_score: float, note: str):
        msg = WelfareMsg(id=pig_id, score=welfare_score, note=note)
        event_system.publish(msg, 'welfare_report')
