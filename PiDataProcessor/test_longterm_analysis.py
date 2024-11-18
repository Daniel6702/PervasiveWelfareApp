import time
import random
from datetime import datetime, timedelta
from DataProcessing.LongTermAnalysis import LongTermAnalysisModule
from Models.MovementData import MovementData
from Models.WelfareMsg import WelfareMsg
 #Initialize the long-term analysis module without starting the daily thread
analysis_module = LongTermAnalysisModule(run_daily_thread=False)

# List of pig IDs
pig_ids = ['pig1', 'pig2', 'pig3']

# Simulate data for 21 days (3 weeks)
num_days = 21

# Starting date (21 days ago)
start_date = datetime.now() - timedelta(days=num_days)

for day_offset in range(num_days):
    current_date = start_date + timedelta(days=day_offset)
    # Set the simulated date's midnight timestamp
    day_midnight = datetime.combine(current_date, datetime.min.time())
    timestamp_start = day_midnight.timestamp()

    print(f"Simulating data for {current_date.strftime('%Y-%m-%d')}")

    for pig_id in pig_ids:
        # Simulate movement data every minute (e.g., every 10 minutes for faster simulation)
        for minute in range(0, 1440, 10):  # Adjusted to every 10 minutes for performance
            movement_timestamp = timestamp_start + minute * 60  # Increment by one minute

            movement_data = MovementData(
                timestamp=movement_timestamp,
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
                pig_id=pig_id
            )

            # Create a fake MQTT message
            class FakeMQTTMessage:
                def __init__(self, topic, payload):
                    self.topic = topic
                    self.payload = payload

            msg = FakeMQTTMessage(
                topic=f"{pig_id}/data",
                payload=movement_data.to_json().encode('utf-8')
            )
            analysis_module.process_data(msg)

        # Simulate welfare scores every hour (24 times per day)
        for hour in range(24):
            welfare_timestamp = timestamp_start + hour * 3600  # Increment by one hour

            welfare_msg = WelfareMsg(
                id=pig_id,
                score=random.uniform(0, 1),
                note=f"Note for {pig_id} on day {day_offset + 1}"
            )

            analysis_module.get_welfare_score(welfare_msg, date_str=datetime.fromtimestamp(welfare_timestamp).strftime('%Y-%m-%d'))

            # After simulating a day's data, aggregate and save the data
    # Provide the current simulated date to the aggregation method
    analysis_module.aggregate_and_save_data(simulated_date=current_date)

# After simulation, publish all aggregated data
analysis_module.publish_longterm_data()

print("Simulation complete. Aggregated data has been published.")