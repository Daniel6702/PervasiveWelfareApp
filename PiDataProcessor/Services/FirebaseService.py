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

    def upload_data(self, collection: str, data: dict) -> None:
        '''Upload data to Firestore'''
        self.db.collection(collection).add(data)

    def retrieve_data(self, collection: str) -> list[dict]:
        '''Retrieve data from Firestore'''
        data = []
        docs = self.db.collection(collection).stream()
        for doc in docs:
            data.append(doc.to_dict())
        return data