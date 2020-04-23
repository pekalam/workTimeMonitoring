using Infrastructure;
using Infrastructure.Messaging;
using MahApps.Metro.Controls;
using Prism.Events;
using Prism.Regions;
using WindowUI.LoginWindow;
using WindowUI.MainWindow;

namespace WindowUI
{
    internal class WindowModuleStartupService
    {
        private readonly IEventAggregator _ea;
        private readonly IRegionManager _regionManager;

        public WindowModuleStartupService(IEventAggregator ea, IRegionManager regionManager)
        {
            _ea = ea;
            _regionManager = regionManager;
            _ea.GetEvent<AppStartedEvent>().Subscribe(OnAppStarted);
        }

        public static MetroWindow ShellWindow { get; private set; }

        private void OnAppStarted(MetroWindow shellWindow)
        {
            ShellWindow = shellWindow;
            _regionManager.Regions[ShellRegions.MainRegion].RequestNavigate(nameof(LoginView));
        }
    }
}