using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;
using Windows.UI.Notifications;
using Infrastructure.Messaging;
using XmlDocument = Windows.Data.Xml.Dom.XmlDocument;

namespace NotificationsW8
{
    internal static class NotificationService
    {
        private static readonly ToastNotifier _toastNotifier;

        static NotificationService()
        {
            _toastNotifier = ToastNotificationManager.CreateToastNotifier();
        }

        private static string GetNotificationStr(NotificationConfig config)
        {
            return $@"<toast duration='long'><visual>
            <binding template='ToastGeneric'>
            <text hint-maxLines='1'>{config.Title}</text>
            <text>{config.Msg}</text>
            </binding>
            </visual>
            <actions>
            <action content='Ok' activationType='background' arguments='dismiss'/>
            </actions>
            <audio silent='true'/></toast>";
        }

        public static void Show(NotificationConfig config)
        {
            var xmlDoc = new XmlDocument();
            xmlDoc.LoadXml(GetNotificationStr(config));

            var toastNotification = new ToastNotification(xmlDoc);

            _toastNotifier.Show(toastNotification);
        }
    }
}
