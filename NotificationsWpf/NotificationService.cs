using Infrastructure.Messaging;
using Notifications.Wpf;

namespace NotificationsWpf
{
    internal static class NotificationService
    {
        private static NotificationType GetNotificationType(NotificationConfig config)
        {
            switch (config.Scenario)
            {
                case NotificationScenario.Information:
                    return NotificationType.Information;
                case NotificationScenario.Warning:
                    return NotificationType.Warning;
                default:
                    return NotificationType.Information;
            }
        }

        public static void Show(NotificationConfig config)
        {
            var notificationManager = new NotificationManager();

            notificationManager.Show(new NotificationContent
            {
                Title = config.Title,
                Message = config.Msg,
                Type = GetNotificationType(config)
            });
        }
    }
}