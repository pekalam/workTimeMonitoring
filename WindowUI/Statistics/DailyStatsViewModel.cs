using System;
using System.Text;
using LiveCharts;
using Prism.Mvvm;
using Prism.Regions;

namespace WindowUI.Statistics
{
    public class DailyStatsViewModel : BindableBase, INavigationAware
    {
        private SeriesCollection _applicationsSeries = new SeriesCollection();
        private DateTime _selectedDate = DateTime.Now;
        private bool _isShowingStats = true;

        public IDailyStatsViewController Controller { get; set; }

        public DateTime SelectedDate
        {
            get => _selectedDate;
            set => SetProperty(ref _selectedDate, value);
        }

        public SeriesCollection ApplicationsSeries
        {
            get => _applicationsSeries;
            set => SetProperty(ref _applicationsSeries, value);
        }

        public bool IsShowingStats
        {
            get => _isShowingStats;
            set => SetProperty(ref _isShowingStats, value);
        }

        public void OnNavigatedTo(NavigationContext navigationContext)
        {
            Controller.Init(this);
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
