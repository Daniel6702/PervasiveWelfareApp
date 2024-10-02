import json
import os
from google.oauth2 import service_account
from google.auth.transport.requests import Request
import requests
from firebase_admin import credentials, firestore, storage, initialize_app
from PIL import Image
from Models.Message import Message

class FirebaseService:
    def __init__(self, credentials_path: str, storage_bucket: str, project_id: str):
        self.credentials_path = credentials_path
        self.storage_bucket = storage_bucket
        self.project_id = project_id

        # Initialize Firebase Admin SDK for Firestore and Storage
        self._initialize_firebase()
        self.db = firestore.client()
        self.bucket = storage.bucket()

        # Load service account credentials for OAuth 2.0
        self.scopes = ['https://www.googleapis.com/auth/firebase.messaging']
        self.credentials_oauth = service_account.Credentials.from_service_account_file(
            credentials_path, scopes=self.scopes)

        # FCM Endpoint
        self.fcm_endpoint = f'https://fcm.googleapis.com/v1/projects/{self.project_id}/messages:send'

    def _initialize_firebase(self):
        cred = credentials.Certificate(self.credentials_path)
        initialize_app(cred, {
            'storageBucket': self.storage_bucket
        })

    def get_access_token(self) -> str:
        self.credentials_oauth.refresh(Request())
        return self.credentials_oauth.token

    def upload_image(self, image_path: str, image_url: str) -> str:
        if not os.path.exists(image_path):
            img = Image.new('RGB', (100, 100), color=(73, 109, 137))
            img.save(image_path)

        blob = self.bucket.blob(image_url)
        blob.upload_from_filename(image_path)
        blob.make_public()  
        return blob.public_url

    def get_device_tokens(self) -> list[str]:
        tokens = []
        users_ref = self.db.collection('users')
        docs = users_ref.stream()
        for doc in docs:
            data = doc.to_dict()
            token = data.get('fcm_token')
            if token:
                tokens.append(token)
        return tokens

    def send_fcm_message(self, message: Message, device_token: str) -> None:
        '''Send FCM message to a device token'''
        fcm_message = message.to_fcm_json(device_token)  

        access_token = self.get_access_token()
        headers = {
            'Authorization': f'Bearer {access_token}',
            'Content-Type': 'application/json; UTF-8'
        }

        response = requests.post(self.fcm_endpoint, headers=headers, data=json.dumps(fcm_message))

        if response.status_code == 200:
            print(f'FCM message sent successfully to {message.token}.')
        else:
            print(f'Failed to send FCM message to {message.token}: {response.status_code} {response.text}')

    def send_message(self, message: Message) -> None:
        '''Send FCM message to all device tokens'''
        device_tokens = self.get_device_tokens()
        for token in device_tokens:
            self.send_fcm_message(message, token)

    def upload_data(self, collection: str, data: dict) -> None:
        '''Upload animal data to Firestore'''
        self.db.collection(collection).add(data)