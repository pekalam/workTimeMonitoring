using Prism.Events;
using UI.Common.Notifications;

namespace UI.Common
{
    public class ShowNotificationEvent : PubSubEvent<NotificationConfig> { }

    public class FaceRecogTriggeredEvent : PubSubEvent
    {

    }

    public class LoadNotificationsModuleEvent : PubSubEvent { }
}