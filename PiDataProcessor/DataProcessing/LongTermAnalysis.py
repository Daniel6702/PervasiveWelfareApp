from datetime import datetime
from collections import defaultdict
from threading import Timer

import numpy as np

from EventSystem import event_system

class LongTermAnalysisModule:
    def __init__(self, analysis_interval=60):
        event_system.subscribe(self.process_data, 'message_received')
        self.aggregated_data = defaultdict(list)
        self.analysis_interval = analysis_interval

    def process_data(self, data):
        pig_id = data['pig_id']

        # Example: Aggregate time spent in each state
        self.aggregated_data[pig_id].append(data)

        # If needed, call functions to compute welfare score or other metrics
        if len(self.aggregated_data[pig_id]) > 100:  # Example: Process every 100 entries
            self.compute_metrics(pig_id)

    def compute_metrics(self, pig_id):
        data = self.aggregated_data[pig_id]
        states = [entry['calc_movement_rf'] for entry in data]

        # Example: Calculate average movement
        avg_movement = np.mean([entry['distance'] for entry in data])

        # Example: State transition probabilities
        transition_counts = defaultdict(int)
        for i in range(1, len(states)):
            transition_counts[(states[i - 1], states[i])] += 1
        transition_probs = {k: v / sum(transition_counts.values()) for k, v in transition_counts.items()}

        # Publish insights
        self.publish_insights(pig_id, avg_movement, transition_probs)

        # Clear data to avoid memory overload
        self.aggregated_data[pig_id] = []


    def publish_insights(self, pig_id, avg_movement, transition_probs):
        """Publish computed insights to the output topic."""
        insight = {
            "timestamp": datetime.now().isoformat(),
            "pig_id": pig_id,
            "avg_movement": avg_movement,
            "transition_probs": transition_probs
        }
        print(insight)
        event_system.publish(insight, 'long_term_analysis')

    def schedule_periodic_analysis(self):
        """Schedule periodic analysis."""
        for pig_id in list(self.aggregated_data.keys()):
            self.compute_metrics(pig_id)
        Timer(self.analysis_interval, self.schedule_periodic_analysis).start()