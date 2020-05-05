using System;
using Infrastructure.Messaging;
using Prism.Events;
using Prism.Ioc;
using Prism.Modularity;
using Prism.Regions;

namespace NotificationsWpf
{
    public class NotificationsWpfModule : IModule
    {
        public void OnInitialized(IContainerProvider containerProvider)
        {
            var ea = containerProvider.Resolve<IEventAggregator>();

            ea.GetEvent<ShowNotificationEvent>().Subscribe(NotificationService.Show, true);

            ea.GetEvent<ShowNotificationEvent>().Publish(new NotificationConfig()
            {
                Msg = "asd",
                Scenario = NotificationScenario.WarningTrigger,
                Title = "add"
            });
        }

        public void RegisterTypes(IContainerRegistry containerRegistry)
        {

        }
    }
}