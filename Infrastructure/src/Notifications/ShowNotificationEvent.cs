using Prism.Events;

namespace Infrastructure.Messaging
{
    public class ShowNotificationEvent : PubSubEvent<NotificationConfig> { }
}