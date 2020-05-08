using MahApps.Metro.Controls;
using Prism.Events;
using Prism.Regions;
using UI.Common;
using UI.Common.Messaging;
using WindowUI.LoginWindow;
using WindowUI.SplashScreen;

namespace WindowUI
{
    internal class WindowModuleStartupService
    {
        private readonly IEventAggregator _ea;
        private readonly IRegionManager _regionManager;

        private SubscriptionToken _appStart;
        private SubscriptionToken _hideSplash;

        public WindowModuleStartupService(IEventAggregator ea, IRegionManager regionManager)
        {
            _ea = ea;
            _regionManager = regionManager;
            _appStart = _ea.GetEvent<AppStartedEvent>().Subscribe(OnAppStarted);
            _hideSplash = _ea.GetEvent<HideSplashScreenEvent>().Subscribe(() =>
            {
                _regionManager.Regions[ShellRegions.MainRegion].RequestNavigate(nameof(LoginView));
                _ea.GetEvent<HideSplashScreenEvent>().Unsubscribe(_hideSplash);
            }, true);
        }

        public static MetroWindow ShellWindow { get; private set; }

        private void OnAppStarted(MetroWindow shellWindow)
        {
            ShellWindow = shellWindow;
            _regionManager.Regions[ShellRegions.MainRegion].RequestNavigate(nameof(SplashScreenView));

            _ea.GetEvent<AppStartedEvent>().Unsubscribe(_appStart);
        }
    }
}