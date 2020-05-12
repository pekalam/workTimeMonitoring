using System.Threading.Tasks;
using Prism.Events;
using Prism.Ioc;
using Prism.Modularity;
using UI.Common;
using UI.Common.Notifications;

namespace NotificationsWpf
{
    public class NotificationsWpfModule : IModule
    {
        public void OnInitialized(IContainerProvider containerProvider)
        {
            var ea = containerProvider.Resolve<IEventAggregator>();

            ea.GetEvent<ShowNotificationEvent>().Subscribe(NotificationService.Show, true);
            ea.GetEvent<HideNotificationsEvent>().Subscribe(NotificationService.HideAll);
        }

        public void RegisterTypes(IContainerRegistry containerRegistry)
        {

        }
    }
}