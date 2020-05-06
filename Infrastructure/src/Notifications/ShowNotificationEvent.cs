using Prism.Events;

namespace Infrastructure.Messaging
{
    public class ShowNotificationEvent : PubSubEvent<NotificationConfig> { }

    public class FaceRecogTriggeredEvent : PubSubEvent
    {

    }
}