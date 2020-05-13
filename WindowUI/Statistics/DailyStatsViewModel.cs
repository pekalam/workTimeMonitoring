using LiveCharts;
using Prism.Mvvm;
using Prism.Regions;
using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Input;

namespace WindowUI.Statistics
{
    public class DailyStatsViewModel : BindableBase, INavigationAware
    {
        private SeriesCollection _applicationsSeries = new SeriesCollection();
        private DateTime _selectedDate = DateTime.Now;
        private bool _isShowingStats = true;
        private bool _showAll = true;
        private Visibility _showAllVisibility;
        private SeriesPickerViewModel _seriesPickerViewModel = new SeriesPickerViewModel();
        private ICommand? _refresh;
        private IDailyStatsViewController? _controller;

        public IDailyStatsViewController? Controller
        {
            get => _controller;
            set
            {
                _controller = value;
                Refresh = value?.Refresh;
            }
        }

        public bool IsDirty { get; set; }

        public ICommand? Refresh
        {
            get => _refresh;
            set => SetProperty(ref _refresh, value);
        }

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

        public bool ShowAll
        {
            get => _showAll;
            set => SetProperty(ref _showAll, value);
        }

        public Visibility ShowAllVisibility
        {
            get => _showAllVisibility;
            set => SetProperty(ref _showAllVisibility, value);
        }

        public SeriesPickerViewModel SeriesPickerViewModel
        {
            get => _seriesPickerViewModel;
            set => SetProperty(ref _seriesPickerViewModel, value);
        }

        public void OnNavigatedTo(NavigationContext navigationContext)
        {
            Debug.Assert(Controller != null);
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
