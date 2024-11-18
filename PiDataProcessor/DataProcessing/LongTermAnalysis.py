from EventSystem import event_system
import paho.mqtt.client as mqtt
import json
import sqlite3
from datetime import datetime, timedelta
import threading
import time
from dataclasses import dataclass, asdict
from Models.WelfareMsg import WelfareMsg
from Models.MovementData import MovementData

class LongTermAnalysisModule:
    def __init__(self, run_daily_thread=True):
        # Initialize in-memory storage for daily aggregation
        self.daily_data = {}
        self.lock = threading.RLock()  # Changed from Lock() to RLock()

        # Initialize database connection
        self.conn = sqlite3.connect('longterm_analysis.db', check_same_thread=False)
        self.cursor = self.conn.cursor()
        self.create_tables()

        # Subscribe to events
        event_system.subscribe(self.process_data, "message_received")
        event_system.subscribe(self.get_welfare_score, "welfare_score_published")

        # Start a thread to handle daily aggregation and publishing if required
        if run_daily_thread:
            self.daily_thread = threading.Thread(target=self.daily_task)
            self.daily_thread.daemon = True
            self.daily_thread.start()

    def create_tables(self):
        # Create AggregatedData table
        self.cursor.execute('''
            CREATE TABLE IF NOT EXISTS AggregatedData (
                date TEXT,
                pig_id TEXT,
                average_welfare_score REAL,
                total_distance_moved REAL,
                time_laying REAL,
                time_standing REAL,
                time_moving REAL,
                keeper_presence_duration REAL,
                average_frames_between_walking REAL,
                PRIMARY KEY (date, pig_id)
            )
        ''')
        self.conn.commit()

    def process_data(self, msg: mqtt.MQTTMessage):
        '''
        Subscribed to the message_received event (movement data) (~1 p sec)
        '''
        pig_id = msg.topic.split('/')[0]
        if msg.topic.endswith("data"):
            self.handle_data(msg.payload, pig_id)

    def handle_data(self, data_bytes, pig_id):
        try:
            json_data = json.loads(data_bytes)
            movement_data = MovementData.from_dict(json_data)
            movement_data.pig_id = pig_id

            # Update daily aggregates
            self.update_daily_data(movement_data)

        except Exception as e:
            print(f"Error processing data: {e}")

    def update_daily_data(self, movement_data):
        with self.lock:
            date_str = datetime.fromtimestamp(movement_data.timestamp).strftime('%Y-%m-%d')
            pig_id = movement_data.pig_id

            # Initialize data structures if not present
            if date_str not in self.daily_data:
                self.daily_data[date_str] = {}
            if pig_id not in self.daily_data[date_str]:
                self.daily_data[date_str][pig_id] = {
                    'total_distance': 0.0,
                    'time_laying': 0.0,
                    'time_standing': 0.0,
                    'time_moving': 0.0,
                    'keeper_presence_duration': 0.0,
                    'frames_between_walking': [],
                    'last_walking_frame': movement_data.last_walking,
                    'last_timestamp': movement_data.timestamp
                }

            pig_data = self.daily_data[date_str][pig_id]

            # Calculate time delta since last data point
            time_delta = movement_data.timestamp - pig_data['last_timestamp']

            # Update total distance moved
            pig_data['total_distance'] += movement_data.distance

            # Update time spent in each activity
            if movement_data.calc_movement_rf == 1:
                pig_data['time_laying'] += time_delta
            elif movement_data.calc_movement_rf == 2:
                pig_data['time_standing'] += time_delta
            elif movement_data.calc_movement_rf == 3:
                pig_data['time_moving'] += time_delta

            # Update keeper presence duration
            if movement_data.keeper_presence_object_detect == 1:
                pig_data['keeper_presence_duration'] += time_delta

            # Update frames between last walking
            pig_data['frames_between_walking'].append(movement_data.last_walking)

            # Update last values
            pig_data['last_timestamp'] = movement_data.timestamp

    def get_welfare_score(self, msg: WelfareMsg, date_str=datetime.fromtimestamp(time.time()).strftime('%Y-%m-%d')):
        '''
        Subscribed to the welfare score publishing event (1 p hour)
        '''
        try:
            self.update_welfare_score(msg, date_str)
        except Exception as e:
            print(f"Error processing welfare score: {e}")

    def update_welfare_score(self, welfare_msg, date_str):
        with self.lock:
            pig_id = welfare_msg.id

            # Initialize data structures if not present
            if date_str not in self.daily_data:
                self.daily_data[date_str] = {}
            if pig_id not in self.daily_data[date_str]:
                self.daily_data[date_str][pig_id] = {
                    'welfare_scores': [],
                    'total_distance': 0.0,
                    'time_laying': 0.0,
                    'time_standing': 0.0,
                    'time_moving': 0.0,
                    'keeper_presence_duration': 0.0,
                    'frames_between_walking': []
                }

            pig_data = self.daily_data[date_str][pig_id]

            # Append welfare score
            if 'welfare_scores' not in pig_data:
                pig_data['welfare_scores'] = []
            pig_data['welfare_scores'].append(welfare_msg.score)


    def daily_task(self):
        while True:
            now = datetime.now()
            # Calculate time until midnight
            next_midnight = (now + timedelta(days=1)).replace(hour=0, minute=0, second=0, microsecond=0)
            sleep_seconds = (next_midnight - now).total_seconds()
            print(f"Sleeping until {next_midnight.strftime('%Y-%m-%d %H:%M:%S')} ({sleep_seconds} seconds)")
            time.sleep(sleep_seconds)
            self.aggregate_and_save_data()

    def aggregate_and_save_data(self, simulated_date=None):
        with self.lock:
            if simulated_date:
                date_str = simulated_date.strftime('%Y-%m-%d')
            else:
                date_str = (datetime.now() - timedelta(days=1)).strftime('%Y-%m-%d')
            if date_str in self.daily_data:
                for pig_id, pig_data in self.daily_data[date_str].items():
                    # Compute aggregates
                    average_welfare_score = None
                    if 'welfare_scores' in pig_data and pig_data['welfare_scores']:
                        average_welfare_score = sum(pig_data['welfare_scores']) / len(pig_data['welfare_scores'])

                    total_distance_moved = pig_data.get('total_distance', 0.0)
                    time_laying = pig_data.get('time_laying', 0.0)
                    time_standing = pig_data.get('time_standing', 0.0)
                    time_moving = pig_data.get('time_moving', 0.0)
                    keeper_presence_duration = pig_data.get('keeper_presence_duration', 0.0)

                    frames_between_walking = pig_data.get('frames_between_walking', [])
                    average_frames_between_walking = None
                    if frames_between_walking:
                        average_frames_between_walking = sum(frames_between_walking) / len(frames_between_walking)

                    # Save to database
                    self.cursor.execute('''
                        INSERT OR REPLACE INTO AggregatedData (
                            date,
                            pig_id,
                            average_welfare_score,
                            total_distance_moved,
                            time_laying,
                            time_standing,
                            time_moving,
                            keeper_presence_duration,
                            average_frames_between_walking
                        ) VALUES (?, ?, ?, ?, ?, ?, ?, ?, ?)
                    ''', (
                        date_str,
                        pig_id,
                        average_welfare_score,
                        total_distance_moved,
                        time_laying,
                        time_standing,
                        time_moving,
                        keeper_presence_duration,
                        average_frames_between_walking
                    ))
                self.conn.commit()

                # Publish aggregated data via event system
                self.publish_longterm_data(date_str)

                # Remove the aggregated day's data
                del self.daily_data[date_str]

    def publish_longterm_data(self, date_str=None):
        with self.lock:
            # Retrieve aggregated data
            if date_str:
                self.cursor.execute('SELECT * FROM AggregatedData WHERE date=?', (date_str,))
            else:
                self.cursor.execute('SELECT * FROM AggregatedData')
            aggregated_data = self.cursor.fetchall()
            columns = [description[0] for description in self.cursor.description]
            aggregates_list = [dict(zip(columns, row)) for row in aggregated_data]

            # Publish data via event system
            event_system.publish({'aggregated_data': aggregates_list},'longterm_data')

    def publish_data_on_request(self):
        # Publish all long-term data when called
        self.publish_longterm_data()