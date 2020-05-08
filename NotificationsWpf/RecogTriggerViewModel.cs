using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;
using System.Windows.Input;
using Notifications.Wpf;
using Prism.Commands;
using Prism.Events;
using UI.Common;

namespace NotificationsWpf
{
    public class RecogTriggerViewModel
    {
        private IEventAggregator _ea;

        public RecogTriggerViewModel(IEventAggregator ea)
        {
            _ea = ea;
            TriggerFaceRecog = new DelegateCommand(() => _ea.GetEvent<FaceRecogTriggeredEvent>().Publish());
        }

        public NotificationContent Content { get; set; }

        public ICommand TriggerFaceRecog { get; set; }
    }
}