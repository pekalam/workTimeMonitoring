using CommonServiceLocator;
using Infrastructure;
using NotificationsWpf;
using Prism.Events;
using Prism.Ioc;
using Prism.Modularity;
using Prism.Unity;
using System.Windows;
using UI.Common;
using UI.Common.Messaging;
using WindowUI;

namespace Application
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : PrismApplication
    {
        public App()
        {
        }

        protected override void ConfigureModuleCatalog(IModuleCatalog moduleCatalog)
        {
            base.ConfigureModuleCatalog(moduleCatalog);
            moduleCatalog.AddModule<InfrastructureModule>();
            moduleCatalog.AddModule<UiCommonModule>();
            moduleCatalog.AddModule<WindowUiModule>();
#if MSIX_RELEASE
            moduleCatalog.AddModule<NotificationsW8Module>();
#else
            moduleCatalog.AddModule<NotificationsWpfModule>();
#endif
        }

        protected override void RegisterTypes(IContainerRegistry containerRegistry)
        {
        }

        protected override Window CreateShell()
        {
            return Container.Resolve<Shell>();
        }

        protected override void OnSessionEnding(SessionEndingCancelEventArgs e)
        {
            ServiceLocator.Current.GetInstance<IEventAggregator>().GetEvent<AppShuttingDownEvent>().Publish();
            base.OnSessionEnding(e);
        }
    }
}
