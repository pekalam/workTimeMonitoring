using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using Domain.Repositories;
using Domain.User;
using Domain.WorkTimeAggregate;
using LiveCharts;
using Prism.Mvvm;
using Prism.Regions;

namespace WindowUI.Statistics
{
    public class DailyStatsViewModel : BindableBase, INavigationAware
    {
        private SeriesCollection _applicationsSeries = new SeriesCollection();
        private readonly IDailyStatsViewController _controller;
        private DateTime _selectedDate = DateTime.Now;
        private bool _isShowingStats = true;

        public DailyStatsViewModel(IDailyStatsViewController controller)
        {
            _controller = controller;
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

    public interface IDailyStatsViewController
    {
        void Init(DailyStatsViewModel vm);
    }

    public class DailyStatsViewController : IDailyStatsViewController
    {
        private readonly IWorkTimeEsRepository _repository;
        private readonly IAuthenticationService _authenticationService;
        private DailyStatsViewModel _vm;

        public DailyStatsViewController(IWorkTimeEsRepository repository, IAuthenticationService authenticationService)
        {
            _repository = repository;
            _authenticationService = authenticationService;
        }

        public void Init(DailyStatsViewModel vm)
        {
            _vm = vm;

            _vm.PropertyChanged += VmOnPropertyChanged;
        }

        private void SelectFromDay()
        {
            var start = new DateTime(_vm.SelectedDate.Year, _vm.SelectedDate.Month, _vm.SelectedDate.Day);
            var end = new DateTime(_vm.SelectedDate.Year, _vm.SelectedDate.Month, _vm.SelectedDate.Day, 23, 59, 59);

            List<WorkTime> workTimes = _repository.FindAll(_authenticationService.User, start.ToUniversalTime(), end.ToUniversalTime());

            if (workTimes.Count == 0)
            {
                _vm.IsShowingStats = false;
                return;
            }
            _vm.IsShowingStats = true;

            var series = workTimes.ToApplicationsPieSeries();
            _vm.ApplicationsSeries.Clear();
            _vm.ApplicationsSeries.AddRange(series);
        }

        private void VmOnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(DailyStatsViewModel.SelectedDate):
                    SelectFromDay();
                    break;
            }
        }
    }
}
