using Prism.Events;
using Prism.Ioc;
using Prism.Modularity;
using UI.Common;

namespace NotificationsW8
{
    public class NotificationsW8Module : IModule
    {
        public void OnInitialized(IContainerProvider containerProvider)
        {
            var ea = containerProvider.Resolve<IEventAggregator>();

            ea.GetEvent<ShowNotificationEvent>().Subscribe(NotificationService.Show, true);
        }

        public void RegisterTypes(IContainerRegistry containerRegistry)
        {

        }
    }
}