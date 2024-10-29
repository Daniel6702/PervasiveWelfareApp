using Plugin.FirebasePushNotifications;

namespace WelfareMonitorApp.ViewModels
{
    public class NotificationCategoryViewModel
    {
        public NotificationCategoryViewModel(NotificationCategory notificationCategory)
        {
            this.Category = notificationCategory.CategoryId;
            this.Actions = notificationCategory.Actions;
        }

        public string Category { get; }

        public NotificationAction[] Actions { get; }
    }
}