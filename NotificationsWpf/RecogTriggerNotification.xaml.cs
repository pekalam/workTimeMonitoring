using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using MahApps.Metro.Controls;
using Notifications.Wpf.Controls;

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
