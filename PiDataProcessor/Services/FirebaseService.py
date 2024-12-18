import json
import os
from google.oauth2 import service_account
from google.auth.transport.requests import Request
import requests
from firebase_admin import credentials, firestore, storage, initialize_app
from PIL import Image
from Models.Message import Message
import warnings

from Models.LTAData import LTAData

warnings.filterwarnings("ignore", category=UserWarning)
from Models.MovementData import MovementData
import time
import tempfile
from Models.ImageMsg import ImgMsg
from Models.WelfareMsg import WelfareMsg

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
    
    def upload_pig_image(self, pig_id: str, image_path: str):
        """
        Uploads an image to Firebase Storage with a unique name and updates Firestore
        with the new image URL for the specified pig_id.
        """
        # Append a unique timestamp to the filename to prevent caching
        timestamp = int(time.time())
        image_name = f"{pig_id}_{timestamp}.jpg"  # Use jpg or png based on your images

        # Upload the image and get the public URL
        image_url = self.upload_image(image_path, image_name)

        # Reference to the 'images' collection in Firestore
        images_collection = self.db.collection('images')

        # Query for existing document with the same pig_id
        query = images_collection.where("pig_id", "==", pig_id).limit(1)
        docs = list(query.stream())

        if docs:
            # Update existing document with new image URL
            doc_ref = images_collection.document(docs[0].id)
            doc_ref.update({
                'image_url': image_url,
                'timestamp': firestore.SERVER_TIMESTAMP
            })
        else:
            # Create new document if it doesn't exist
            images_collection.add({
                'pig_id': pig_id,
                'image_url': image_url,
                'timestamp': firestore.SERVER_TIMESTAMP
            })

    def upload_pig_data(self, movement_data: MovementData):
        """
        Uploads or updates movement data for a pig in Firestore.
        """
        # Reference to the 'movement_data' collection in Firestore
        movement_collection = self.db.collection('movement_data')

        pig_id = movement_data.pig_id

        if not pig_id:
            raise ValueError("Pig ID is required for uploading movement data.")

        # Query for existing document with the same pig_id
        query = movement_collection.where("pig_id", "==", pig_id).limit(1)
        docs = list(query.stream())

        data_dict = movement_data.to_dict()
        data_dict['timestamp'] = firestore.SERVER_TIMESTAMP  # Overwrite timestamp with server time

        if docs:
            # Update existing document with new movement data
            doc_ref = movement_collection.document(docs[0].id)
            doc_ref.update(data_dict)
        else:
            # Create new document if it doesn't exist
            movement_collection.add(data_dict)


    def upload_pig_image_msg(self, msg: ImgMsg):
        """
        Alternative to upload_pig_image that accepts an ImgMsg dataclass instance.
        Uploads the image bytes to Firebase Storage and updates Firestore with the image URL.
        
        Parameters:
            msg (ImgMsg): The image message containing image bytes and pig ID.
        """
        # Create a temporary file to store the image bytes
        with tempfile.NamedTemporaryFile(suffix='.jpg', delete=False) as temp_file:
            temp_file.write(msg.img)
            temp_file_path = temp_file.name

        try:
            # Generate a unique image name using pig ID and timestamp
            timestamp = int(time.time())
            image_name = f"{msg.id}_{timestamp}.jpg"

            # Upload the image using the existing upload_image method
            image_url = self.upload_image(temp_file_path, image_name)
        finally:
            # Ensure the temporary file is removed after upload
            os.remove(temp_file_path)

        # Reference to the 'images' collection in Firestore
        images_collection = self.db.collection('images')

        # Query for existing document with the same pig_id
        query = images_collection.where("pig_id", "==", msg.id).limit(1)
        docs = list(query.stream())

        if docs:
            # Update existing document with new image URL
            doc_ref = images_collection.document(docs[0].id)
            doc_ref.update({
                'image_url': image_url,
                'timestamp': firestore.SERVER_TIMESTAMP
            })
        else:
            # Create new document if it doesn't exist
            images_collection.add({
                'pig_id': msg.id,
                'image_url': image_url,
                'timestamp': firestore.SERVER_TIMESTAMP
            })

    def upload_pig_welfare(self, welfare_msg: WelfareMsg):
        """
        Uploads or updates welfare data for a pig in Firestore.

        Parameters:
            welfare_msg (WelfareMsg): The welfare message containing pig ID, score, and note.
        """
        # Reference to the 'welfare_data' collection in Firestore
        welfare_collection = self.db.collection('welfare_data')

        pig_id = welfare_msg.id

        if not pig_id:
            raise ValueError("Pig ID is required for uploading welfare data.")

        # Query for existing document with the same pig_id
        query = welfare_collection.where("pig_id", "==", pig_id).limit(1)
        docs = list(query.stream())

        data_dict = welfare_msg.to_dict()

        if docs:
            # Update existing document with new welfare data
            doc_ref = welfare_collection.document(docs[0].id)
            doc_ref.update(data_dict)
        else:
            # Create new document if it doesn't exist
            welfare_collection.add(data_dict)

    def upload_lta(self, ltaData: LTAData):
        """
        Uploads insight from the long term analysis.

        Parameters:
            ltaData (LTAData): The long term analysis data.
        """
        lta_collection = self.db.collection('long_term_analysis')

        pig_id = ltaData.pig_id

        if not pig_id:
            raise ValueError("Pig ID is required for uploading long term analysis.")

        lta_collection.add(ltaData.to_dict())