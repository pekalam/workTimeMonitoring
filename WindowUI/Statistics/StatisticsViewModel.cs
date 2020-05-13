using Prism.Mvvm;
using Prism.Regions;
using System;

namespace WindowUI.Statistics
{
    public class StatisticsViewModel : BindableBase, INavigationAware
    {
        private readonly StatisticsViewController _controller;

        public event Action<int> TabChanged;
        private int _selectedInd;

        public StatisticsViewModel(StatisticsViewController controller, OverallStatsViewModel overallStatsViewModel, DailyStatsViewModel dailyStatsViewModel)
        {
            _controller = controller;
            OverallStatsViewModel = overallStatsViewModel;
            DailyStatsViewModel = dailyStatsViewModel;
        }

        public OverallStatsViewModel OverallStatsViewModel { get; set; }
        public DailyStatsViewModel DailyStatsViewModel { get; set; }


        public void OnNavigatedTo(NavigationContext navigationContext)
        {
            TabChanged = delegate{};
            _controller.Init(this);
            OverallStatsViewModel.OnNavigatedTo(navigationContext);
            DailyStatsViewModel.OnNavigatedTo(navigationContext);
        }

        public bool IsNavigationTarget(NavigationContext navigationContext)
        {
            return true;
        }

        public void OnNavigatedFrom(NavigationContext navigationContext)
        {
        }

        public void RaiseTabChanged(int index)
        {
            if (index != _selectedInd)
            {
                TabChanged?.Invoke(index);
                _selectedInd = index;
            }
        }
    }
}