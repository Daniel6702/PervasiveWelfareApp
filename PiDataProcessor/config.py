CREDENTIALS_PATH = 'Credentials/pigwelfaremonitoring-firebase-adminsdk-urg6y-bea4b46852.json'
STORAGE_BUCKET = 'pigwelfaremonitoring.appspot.com'
PROJECT_ID = 'pigwelfaremonitoring'

# Time thresholds (in seconds)
AGGREGATION_PERIOD = 24 * 3600  # 24 hours
ANALYSIS_INTERVAL = 3600        # 1 hour

# Welfare thresholds. Parameters of the normal distributions used to calculate the welfare score
# These thresholds should be defined based on domain knowledge or empirical data
MIN_MOVEMENT_FREQUENCY = 0.1  # Minimum movements per hour
MAX_MOVEMENT_FREQUENCY = 5.0  # Maximum movements per hour

MIN_STANDING_FREQUENCY = 0.5  # Minimum stand-ups per hour
MAX_STANDING_FREQUENCY = 10.0 # Maximum stand-ups per hour

MIN_LAYING_FREQUENCY = 0.5    # Minimum lay-downs per hour
MAX_LAYING_FREQUENCY = 10.0    # Maximum lay-downs per hour

MIN_MOVEMENT_DURATION = 300    # Minimum total movement duration in seconds per hour
MAX_MOVEMENT_DURATION = 7200   # Maximum total movement duration in seconds per hour

MIN_STANDING_DURATION = 1800   # Minimum total standing duration in seconds per hour
MAX_STANDING_DURATION = 21600  # Maximum total standing duration in seconds per hour

MIN_LAYING_DURATION = 1800     # Minimum total laying duration in seconds per hour
MAX_LAYING_DURATION = 21600    # Maximum total laying duration in seconds per hour

MIN_DISTANCE_MOVED = 100.0     # Minimum distance moved per hour (meters, adjust as needed)
MAX_DISTANCE_MOVED = 1000.0    # Maximum distance moved per hour (meters, adjust as needed)


# Weights for each metric in the final welfare score
WEIGHT_MOVEMENT_FREQUENCY = 0.15
WEIGHT_STANDING_FREQUENCY = 0.15
WEIGHT_LAYING_FREQUENCY = 0.15
WEIGHT_MOVEMENT_DURATION = 0.15
WEIGHT_STANDING_DURATION = 0.10
WEIGHT_LAYING_DURATION = 0.10
WEIGHT_DISTANCE_MOVED = 0.20

NOTE_THRESHOLDS = {
    'movement_freq': {
        'low': 0.3,
        'high': 0.7,
        'messages': {
            'low': 'Low movement',
            'high': 'High movement'
        }
    },
    'standing_freq': {
        'high': 0.7,
        'messages': {
            'high': 'High standing frequency'
        }
    },
    'laying_freq': {
        'high': 0.7,
        'messages': {
            'high': 'High laying frequency'
        }
    },
    'movement_dur': {
        'low': 0.3,
        'high': 0.7,
        'messages': {
            'low': 'Low movement duration',
            'high': 'High movement duration'
        }
    },
    'standing_dur': {
        'high': 0.7,
        'messages': {
            'high': 'High standing duration'
        }
    },
    'laying_dur': {
        'high': 0.7,
        'messages': {
            'high': 'High laying duration'
        }
    },
    'distance_moved': {
        'low': 0.3,
        'high': 0.7,
        'messages': {
            'low': 'Low distance moved',
            'high': 'High distance moved'
        }
    }
}