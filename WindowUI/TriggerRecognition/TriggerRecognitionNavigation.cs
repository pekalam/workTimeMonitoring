using Prism.Events;
using Prism.Regions;
using System.Linq;
using System.Windows;
using UI.Common;
using UI.Common.Messaging;
using WMAlghorithm.Services;

namespace WindowUI.TriggerRecognition
{
    internal class TriggerRecognitionNavigation
    {
        private readonly IEventAggregator _ea;
        private readonly IRegionManager _rm;
        private readonly AlgorithmService _moduleService;

        public TriggerRecognitionNavigation(IRegionManager rm, AlgorithmService moduleService, IEventAggregator ea)
        {
            _rm = rm;
            _moduleService = moduleService;
            _ea = ea;
        }

        public void NaviateToTriggerRecogView()
        {
            if (_moduleService.Alghorithm.ManualRecog)
            {
                return;
            }

            var windowOpened = WindowModuleStartupService.ShellWindow.Visibility != Visibility.Hidden;
            if (!windowOpened)
            {
                _ea.GetEvent<ShowWindowEvent>().Publish();
            }

            var previousView = _rm.Regions[ShellRegions.MainRegion].ActiveViews.FirstOrDefault();

            _rm.RequestNavigate(ShellRegions.MainRegion, nameof(TriggerRecognitionView), new NavigationParameters()
            {
                {"WindowOpened", windowOpened},
                {"PreviousView", previousView}
            });
        }
    }
}