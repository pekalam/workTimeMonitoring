using System.Windows;
using System.Windows.Controls;

namespace NotificationsWpf
{
    /// <summary>
    /// Interaction logic for RecogTriggerNotification.xaml
    /// </summary>
    public partial class RecogTriggerNotification : UserControl
    {
        public RecogTriggerNotification()
        {
            InitializeComponent();

        }

        protected override void OnVisualParentChanged(DependencyObject oldParent)
        {
            base.OnVisualParentChanged(oldParent);
            notification.Template = FindResource("NotificationTemplate2") as ControlTemplate;
        }
    }
}
