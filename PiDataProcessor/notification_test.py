from Services.NotificationService import NotificationService
from Credentials.credentials import CREDENTIALS_PATH

FCM_TOKEN = "c7c-sAquQLicUWdzuzni82:APA91bFHp2JI8iI1aD5zQhuovEprLAW49FrXIQN7HT83VWCJucKWFdQ_BfQ4erWmPJ5NkcEtZmONpvHG5SxhjI2UBCHgmgwx0PNwWZzVAaH5zEWf9UOsfmY"

service = NotificationService(CREDENTIALS_PATH)
service.send_to_topic(topic = 'pigs', title = 'Weather Update', body = 'Pleasant with clouds and sun', data ={
    "sunrise": "1684926645",
    "sunset": "1684977332",
   "temp": "292.55",
    "feels_like": "292.87"
})

#service.send_to_token(tokens = FCM_TOKEN, title = 'Weather Update', body = 'Pleasant with clouds and sun', data ={
#    "sunrise": "1684926645",
#    "sunset": "1684977332",
#    "temp": "292.55",
#    "feels_like": "292.87"
#})
