from ingestion import set_data_handler, start_ingestion
from data_processing import DataProcessor

IDS = ['PigPi-1', 'PigPi-2', 'PigPi-3', 'PigPi-4', 'PigPi-5', 'PigPi-6']

data_processor = DataProcessor()

set_data_handler(data_processor)

start_ingestion(
    broker_host='localhost', 
    broker_port=1883, 
    topic='PigPi-1/data', 
    client_id='IngestionClient'
    )