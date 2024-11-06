import firebase_admin
from firebase_admin import credentials, messaging
import logging
from typing import List, Optional, Dict
from Credentials.credentials import CREDENTIALS_PATH
import time
import datetime
from firebase_admin import exceptions

''' Parameters:
Topic: Devices subscribed to this topic will receive the notification.
Tokens: List of device tokens to send the notification to.
data: Custom key-value pairs to be sent in the notification message.
category: iOS-specific category for the notification.
click_action: Android-specific action to be triggered when the notification is tapped.
priority: Priority of the notification (high or normal).
ttl: Time-to-live duration of the notification in seconds.
condition: Condition to send the notification to multiple topics.
token: Device token to send the notification to.
'''

class NotificationService:
    def __init__(self, credentials_path: str):
        cred = credentials.Certificate(credentials_path) 
        firebase_admin.initialize_app(cred)
        self.logger = logging.getLogger(__name__)
    
    def send_to_topic(
        self,
        topic: str,
        title: str,
        body: str,
        data: Optional[Dict[str, str]] = None,
        category: Optional[str] = None,
        click_action: Optional[str] = None,
        priority: Optional[str] = 'high',
        ttl: Optional[int] = None  # TTL in seconds
    ):
        """
        Sends a push notification to a specified topic using Firebase Admin SDK.
        """
        try:
            # Build the AndroidConfig
            android_config = messaging.AndroidConfig(
                priority=priority,
                notification=messaging.AndroidNotification(
                    click_action=click_action
                ) if click_action else None,
                ttl=datetime.timedelta(seconds=ttl) if ttl else None
            )

            # Build the APNSConfig
            apns_config = None
            if category or ttl:
                aps = messaging.Aps()
                if category:
                    aps.category = category
                apns_config = messaging.APNSConfig(
                    payload=messaging.APNSPayload(aps=aps),
                    headers={
                        'apns-expiration': str(int(time.time()) + ttl) if ttl else None
                    }
                )

                # Remove None values from headers
                if apns_config.headers:
                    apns_config.headers = {k: v for k, v in apns_config.headers.items() if v is not None}

            # Build the message
            message = messaging.Message(
                notification=messaging.Notification(
                    title=title,
                    body=body
                ),
                data=data,
                topic=topic,
                android=android_config,
                apns=apns_config
            )

            response = messaging.send(message)
            self.logger.info(f'Message sent to topic "{topic}" successfully: {response}')
        except Exception as e:
            self.logger.error(f'Error sending message to topic "{topic}": {e}')

    def send_to_token(
        self,
        token: str,
        title: str,
        body: str,
        data: Optional[Dict[str, str]] = None,
        category: Optional[str] = None,
        click_action: Optional[str] = None,
        priority: Optional[str] = 'high',
        ttl: Optional[int] = None  # TTL in seconds
    ):
        if not token:
            self.logger.warning("No token provided to send_to_token.")
            return

        try:
            # Build the AndroidConfig
            android_config = messaging.AndroidConfig(
                priority=priority,
                notification=messaging.AndroidNotification(
                    click_action=click_action
                ) if click_action else None,
                ttl=datetime.timedelta(seconds=ttl) if ttl else None
            )

            # Build the APNSConfig
            apns_config = None
            if category or ttl:
                aps = messaging.Aps()
                if category:
                    aps.category = category
                apns_payload = messaging.APNSPayload(aps=aps)
                headers = {}
                if ttl:
                    # Calculate expiration timestamp
                    expiration = str(int(time.time()) + ttl)
                    headers['apns-expiration'] = expiration
                apns_config = messaging.APNSConfig(
                    payload=apns_payload,
                    headers=headers if headers else None
                )

            # Build the message
            message = messaging.Message(
                notification=messaging.Notification(
                    title=title,
                    body=body
                ),
                data=data,
                token=token,
                android=android_config,
                apns=apns_config
            )

            # Send the message
            response = messaging.send(message)
            self.logger.info(f'Message sent to token "{token}" successfully: {response}')

        except messaging.InvalidArgumentError as e:
            self.logger.error(f'Invalid arguments provided for sending message to token "{token}": {e}')
        except Exception as e:
            self.logger.error(f'Error sending message to token "{token}": {e}')

    def subscribe_tokens_to_topic(self, tokens: List[str], topic: str):
        """
        Subscribes a list of device tokens to a specific topic.
        """
        try:
            response = messaging.subscribe_to_topic(tokens, topic)
            self.logger.info(f'Successfully subscribed {response.success_count} tokens to topic "{topic}".')
            if response.failure_count > 0:
                for idx, resp in enumerate(response.errors):
                    self.logger.error(f'Failed to subscribe {tokens[idx]}: {resp.reason}')
        except Exception as e:
            self.logger.error(f'Error subscribing tokens to topic "{topic}": {e}')
    
    def unsubscribe_tokens_from_topic(self, tokens: List[str], topic: str):
        """
        Unsubscribes a list of device tokens from a specific topic.
        """
        try:
            response = messaging.unsubscribe_from_topic(tokens, topic)
            self.logger.info(f'Successfully unsubscribed {response.success_count} tokens from topic "{topic}".')
            if response.failure_count > 0:
                for idx, resp in enumerate(response.errors):
                    self.logger.error(f'Failed to unsubscribe {tokens[idx]}: {resp.reason}')
        except Exception as e:
            self.logger.error(f'Error unsubscribing tokens from topic "{topic}": {e}')
    
    def send_condition_message(
        self,
        condition: str,
        title: str,
        body: str,
        data: Optional[Dict[str, str]] = None,
        priority: Optional[str] = 'high',
        ttl: Optional[int] = None  # Correct type hint
    ):
        """
        Sends a push notification based on a condition involving multiple topics.
        """
        try:
            android_ttl = f"{ttl}s" if ttl is not None else None
            apns_expiration = str(int(time.time()) + ttl) if ttl is not None else None

            message = messaging.Message(
                notification=messaging.Notification(
                    title=title,
                    body=body
                ),
                data=data,
                condition=condition,
                android=messaging.AndroidConfig(
                    priority=priority,
                    ttl=android_ttl
                ),
                apns=messaging.APNSConfig(
                    headers={'apns-expiration': apns_expiration} if apns_expiration else None,
                    payload=messaging.APNSPayload(
                        aps=messaging.Aps()
                    )
                )
            )
            
            response = messaging.send(message)
            self.logger.info(f'Condition-based message sent successfully: {response}')
        except Exception as e:
            self.logger.error(f'Error sending condition-based message: {e}')

    def send_data_only_message(
        self,
        token: str,
        data: Dict[str, str],
        priority: Optional[str] = 'normal',
        ttl: Optional[int] = None  # Correct type hint
    ):
        """
        Sends a data-only (silent) push notification to a specific device token.
        """
        try:
            android_ttl = f"{ttl}s" if ttl is not None else None
            apns_expiration = str(int(time.time()) + ttl) if ttl is not None else None

            message = messaging.Message(
                data=data,
                token=token,
                android=messaging.AndroidConfig(
                    priority=priority,
                    ttl=android_ttl
                ),
                apns=messaging.APNSConfig(
                    headers={'apns-expiration': apns_expiration} if apns_expiration else None,
                    payload=messaging.APNSPayload(
                        aps=messaging.Aps()
                    )
                )
            )
            
            response = messaging.send(message)
            self.logger.info(f'Data-only message sent successfully to token "{token}": {response}')
        except Exception as e:
            self.logger.error(f'Error sending data-only message to token "{token}": {e}')

