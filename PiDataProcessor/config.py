CREDENTIALS_PATH = 'Credentials/pigwelfaremonitoring-firebase-adminsdk-urg6y-bea4b46852.json'
STORAGE_BUCKET = 'pigwelfaremonitoring.appspot.com'
PROJECT_ID = 'pigwelfaremonitoring'

# Time thresholds (in seconds)
AGGREGATION_PERIOD = 24 * 3600  # 24 hours
ANALYSIS_INTERVAL = 3600        # 1 hour

# Gaussian model parameters: means and std devs for each metric per hour
# (These values are examples and must be determined by domain knowledge or historical data)
MEAN_MOVEMENT_FREQUENCY = 2.0
STD_MOVEMENT_FREQUENCY = 0.5

MEAN_STANDING_FREQUENCY = 5.0
STD_STANDING_FREQUENCY = 2.0

MEAN_LAYING_FREQUENCY = 5.0
STD_LAYING_FREQUENCY = 2.0

MEAN_MOVEMENT_DURATION = 1800    # seconds per hour on average
STD_MOVEMENT_DURATION = 600

MEAN_STANDING_DURATION = 10800   # 3 hours standing out of every hour on average? Adjust as needed
STD_STANDING_DURATION = 3600

MEAN_LAYING_DURATION = 10800     # Similarly adjust as needed
STD_LAYING_DURATION = 3600

MEAN_DISTANCE_MOVED = 500.0      # meters per hour
STD_DISTANCE_MOVED = 200.0

# Weights for each metric in the final welfare score
WEIGHT_MOVEMENT_FREQUENCY = 0.15
WEIGHT_STANDING_FREQUENCY = 0.15
WEIGHT_LAYING_FREQUENCY = 0.15
WEIGHT_MOVEMENT_DURATION = 0.15
WEIGHT_STANDING_DURATION = 0.10
WEIGHT_LAYING_DURATION = 0.10
WEIGHT_DISTANCE_MOVED = 0.20

# NOTE_THRESHOLDS now interpreted as probability thresholds
# If probability < low => abnormal
NOTE_THRESHOLDS = {
    'movement_freq': {
        'low': 0.3,
        'messages': {
            'low': 'Unusually low probability of normal movement frequency'
        }
    },
    'standing_freq': {
        'low': 0.3,
        'messages': {
            'low': 'Unusually low probability of normal standing frequency'
        }
    },
    'laying_freq': {
        'low': 0.3,
        'messages': {
            'low': 'Unusually low probability of normal laying frequency'
        }
    },
    'movement_dur': {
        'low': 0.3,
        'messages': {
            'low': 'Unusually low probability of normal movement duration'
        }
    },
    'standing_dur': {
        'low': 0.3,
        'messages': {
            'low': 'Unusually low probability of normal standing duration'
        }
    },
    'laying_dur': {
        'low': 0.3,
        'messages': {
            'low': 'Unusually low probability of normal laying duration'
        }
    },
    'distance_moved': {
        'low': 0.3,
        'messages': {
            'low': 'Unusually low probability of normal distance moved'
        }
    }
}
