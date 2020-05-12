using System;

namespace UI.Common.Notifications
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
        public TimeSpan Length { get; set; } = TimeSpan.FromSeconds(4);
    }
}