using System;
using LiveCharts;
using Prism.Mvvm;
using Prism.Regions;

namespace WindowUI.Statistics
{
    public class OverallStatsViewModel : BindableBase, INavigationAware
    {
        private readonly OverallStatsController _controller;

        private SeriesCollection _pieSeries = new SeriesCollection();
        private int _lowerDate;
        private int _upperDate;
        private int _maxDays;
        private int _minDays;
        private DateTime _maxDate;
        private DateTime _minDate;

        public OverallStatsViewModel(OverallStatsController controller)
        {
            _controller = controller;
        }

        public SeriesCollection PieSeries
        {
            get => _pieSeries;
            set => SetProperty(ref _pieSeries, value);
        }

        public int LowerDate
        {
            get => _lowerDate;
            set => SetProperty(ref _lowerDate, value);
        }

        public int UpperDate
        {
            get => _upperDate;
            set => SetProperty(ref _upperDate, value);
        }

        public int MaxDays
        {
            get => _maxDays;
            set => SetProperty(ref _maxDays, value);
        }

        public int MinDays
        {
            get => _minDays;
            set => SetProperty(ref _minDays, value);
        }

        public DateTime MaxDate
        {
            get => _maxDate;
            set => SetProperty(ref _maxDate, value);
        }

        public DateTime MinDate
        {
            get => _minDate;
            set => SetProperty(ref _minDate, value);
        }


        public void OnNavigatedTo(NavigationContext navigationContext)
        {
            _controller.Init(this);
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