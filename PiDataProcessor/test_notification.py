from Services.NotificationService import NotificationService
from config import *

FCM_TOKEN = "c1H6VpCySLGe_8C-NE1O2p:APA91bGon1FHyQaPp7qN_Gf9vbEIVSe_RDJZ7dyjZjMZc5twtjt5aSUNtjhWXxXtBNkL7qpzS14xpfb8CDB25j2-hdEUtL8HMigZ5LMeBnZuFMWsMN_Egqg"

service = NotificationService(CREDENTIALS_PATH)
service.send_to_topic(topic = 'pigs', title = 'Daily Update', body = 'Pig is healthy', data ={
    "welfare_score": "100"
})

#service.send_to_token(tokens = FCM_TOKEN, title = 'Weather Update', body = 'Pleasant with clouds and sun', data ={
#    "sunrise": "1684926645",
#    "sunset": "1684977332",
#    "temp": "292.55",
#    "feels_like": "292.87"
#})
