﻿using Plugin.FirebasePushNotifications;

namespace WelfareMonitorApp.Services
{
    public class CustomPushNotificationHandler : IPushNotificationHandler
    {
        public CustomPushNotificationHandler()
        {
        }

        public void OnOpened(IDictionary<string, object> parameters, NotificationCategoryType notificationCategoryType)
        {
        }

        public void OnReceived(IDictionary<string, object> parameters)
        {
        }
    }
}