using Notifications.Wpf;
using System.Windows;

namespace NotificationsWpf
{
    public class CustomTemplateSelector : NotificationTemplateSelector
    {
        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            if (item is RecogTriggerViewModel)
            {
                return (container as FrameworkElement)?.FindResource("RecogTriggerTemplate") as DataTemplate;
            }

            return base.SelectTemplate(item, container);
        }
    }
}
