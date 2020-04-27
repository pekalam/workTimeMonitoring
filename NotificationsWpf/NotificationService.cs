using Infrastructure.Messaging;
using Notifications.Wpf;

namespace NotificationsWpf
{
    internal class NotificationService
    {
        public static void Show(NotificationConfig config)
        {
            var notificationManager = new NotificationManager();

            notificationManager.Show(new NotificationContent
            {
                Title = config.Title,
                Message = config.Msg,
                Type = NotificationType.Information
            });
        }
    }
}