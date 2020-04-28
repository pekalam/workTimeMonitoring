using System;
using System.Collections.Generic;
using System.ComponentModel;
using Domain.Repositories;
using Domain.User;
using Domain.WorkTimeAggregate;
using Unity;
using WindowUI.RepoProxy;

namespace WindowUI.Statistics
{
    public interface IDailyStatsViewController
    {
        void Init(DailyStatsViewModel vm);
    }

    public class DailyStatsViewController : IDailyStatsViewController
    {
        private readonly IWorkTimeEsRepository _repository;
        private readonly IAuthenticationService _authenticationService;
        private DailyStatsViewModel _vm;

        public DailyStatsViewController([Dependency(nameof(WorkTimeEsRepositorDecorator))] IWorkTimeEsRepository repository, IAuthenticationService authenticationService)
        {
            _repository = repository;
            _authenticationService = authenticationService;
        }

        public void Init(DailyStatsViewModel vm)
        {
            _vm = vm;

            _vm.PropertyChanged += VmOnPropertyChanged;
        }

        public void UpdateChart()
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
                    UpdateChart();
                    break;
            }
        }
    }
}