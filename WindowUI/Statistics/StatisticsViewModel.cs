using Prism.Mvvm;
using Prism.Regions;

namespace WindowUI.Statistics
{
    public class StatisticsViewModel : BindableBase, INavigationAware
    {
        private readonly StatisticsViewController _controller;
        private bool _isShowingStats = true;

        public StatisticsViewModel(StatisticsViewController controller, OverallStatsViewModel overallStatsViewModel)
        {
            _controller = controller;
            OverallStatsViewModel = overallStatsViewModel;
        }

        public OverallStatsViewModel OverallStatsViewModel { get; set; }

        public bool IsShowingStats
        {
            get => _isShowingStats;
            set => SetProperty(ref _isShowingStats, value);
        }

        public void OnNavigatedTo(NavigationContext navigationContext)
        {
            _controller.Init(this);
            OverallStatsViewModel.OnNavigatedTo(navigationContext);
        }

        public bool IsNavigationTarget(NavigationContext navigationContext)
        {
            return true;
        }

        public void OnNavigatedFrom(NavigationContext navigationContext)
        {
        }
    }
}