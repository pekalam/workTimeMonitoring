namespace Infrastructure.Messaging
{
    public enum NotificationScenario
    {
        Information, Warning, WarningTrigger
    }

    public class NotificationConfig
    {
        public string Title { get; set; }
        public string Msg { get; set; }
        public NotificationScenario Scenario { get; set; }
    }
}