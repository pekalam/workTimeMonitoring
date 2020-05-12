using CommonServiceLocator;
using Notifications.Wpf;
using Prism.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using MahApps.Metro.Controls;
using Notifications.Wpf.Controls;
using UI.Common.Notifications;
using Unity.Injection;

namespace NotificationsWpf
{
    internal static class NotificationService
    {
        private static NotificationsOverlayWindow? _overlayWindow;

        private static NotificationType GetNotificationType(NotificationConfig config)
        {
            switch (config.Scenario)
            {
                case NotificationScenario.Information:
                    return NotificationType.Information;
                case NotificationScenario.Warning:
                    return NotificationType.Warning;
                case NotificationScenario.WarningTrigger:
                    return NotificationType.Warning;
                default:
                    return NotificationType.Information;
            }
        }

        public static void HideAll()
        {
            TryInitOverlayWin();
            Application.Current.Invoke(() =>
            {
                var btns = _overlayWindow?.FindChildren<Button>(true).ToList();

                if (btns == null)
                {
                    return;
                }
                foreach (var button in btns)
                {
                    if (VisualTreeHelper.GetParent(button) is Grid)
                    {
                        button.RaiseEvent(new RoutedEventArgs(ButtonBase.ClickEvent));
                    }
                }
            });
        }

        private static void TryInitOverlayWin()
        {
            if (_overlayWindow == null)
            {
                Application.Current.Invoke(() =>
                {
                    _overlayWindow = (NotificationsOverlayWindow?)
                        Application.Current.Windows.Cast<Window>().SingleOrDefault(w => w is NotificationsOverlayWindow);
                });

            }
        }

        public static void Show(NotificationConfig config)
        {
            TryInitOverlayWin();

            var notificationManager = new NotificationManager();

            if (config.Scenario == NotificationScenario.WarningTrigger)
            {
                notificationManager.Show(
                    new RecogTriggerViewModel(ServiceLocator.Current.GetInstance<IEventAggregator>())
                    {
                        Content = new NotificationContent
                        {
                            Title = config.Title,
                            Message = config.Msg,
                            Type = GetNotificationType(config)
                        }
                    }, expirationTime: config.Length);
            }
            else
            {
                notificationManager.Show(new NotificationContent
                {
                    Title = config.Title,
                    Message = config.Msg,
                    Type = GetNotificationType(config)
                }, expirationTime: config.Length);
            }
        }
    }
}