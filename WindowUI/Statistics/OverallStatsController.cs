﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows.Threading;
using Domain.Repositories;
using Domain.User;
using Domain.WorkTimeAggregate;
using Domain.WorkTimeAggregate.Events;
using LiveCharts;
using LiveCharts.Wpf;

namespace WindowUI.Statistics
{
    public static class StatsPieSeriesExtensions
    {
        public static IEnumerable<PieSeries> ToPieSeries(this IEnumerable<MouseAction> events)
        {
            var series = new PieSeries();
            series.Values = new ChartValues<int>(new int[] {events.Sum(e => e.MkEvent.TotalTime)});
            series.Title = "Mouse";
            return new[] {series};
        }

        public static IEnumerable<PieSeries> ToPieSeries(this IEnumerable<KeyboardAction> events)
        {
            var series = new PieSeries();
            series.Values = new ChartValues<int>(new int[] {events.Sum(e => e.MkEvent.TotalTime)});
            series.Title = "Keyboard";
            return new[] {series};
        }

        public static IEnumerable<PieSeries> ToPieSeries(this IEnumerable<UserWatchingScreen> events)
        {
            var series = new PieSeries();
            series.Values = new ChartValues<long>(new long[] { events.Sum(e => e.TotalTimeMs) });
            series.Title = "Watching screen";
            return new[] { series };
        }

        public static IEnumerable<PieSeries> ToPieSeries(this IEnumerable<FaceRecognitionFailure> events)
        {
            var series = new PieSeries();
            series.Values = new ChartValues<long>(new long[] { events.Sum(e => e.LengthMs) });
            series.Title = "Away";
            return new[] { series };
        }
    }

    public class OverallStatsController
    {
        private OverallStatsViewModel _vm;
        private List<WorkTime> _workTimes;
        private readonly IWorkTimeEsRepository _repository;
        private readonly IAuthenticationService _authenticationService;
        private DateTime _startDate;
        private DateTime _endDate;

        public OverallStatsController(IWorkTimeEsRepository repository,
            IAuthenticationService authenticationService)
        {
            _repository = repository;
            _authenticationService = authenticationService;
        }

        private void SetupDateSlider()
        {
            var first = _workTimes.First();
            var last = _workTimes.Last();


            _startDate = new DateTime(first.DateCreated.Year, first.DateCreated.Month, first.DateCreated.Day);
            _endDate = new DateTime(last.EndDate.Year, last.EndDate.Month, last.EndDate.Day, 23, 59, 59);

            var diff = _endDate - _startDate;


            _vm.MaxDays = (int) diff.TotalDays;
            _vm.MinDays = 0;
            _vm.UpperDate = _vm.MaxDays;
            _vm.LowerDate = _vm.MinDays;
            _vm.MinDate = _startDate;
            _vm.MaxDate = _endDate;
        }

        private void UpdateChart()
        {
            var selected = _workTimes.Where(w => _vm.MinDate <= w.DateCreated && _vm.MaxDate >= w.EndDate).ToList();
            var series = selected
                .SelectMany(w => w.MouseActionEvents).ToPieSeries()
                .Concat(selected.SelectMany(w => w.KeyboardActionEvents).ToPieSeries())
                .Concat(selected.SelectMany(w => w.UserWatchingScreen).ToPieSeries())
                .Concat(selected.SelectMany(s => s.FaceRecognitionFailures).ToPieSeries()).ToList();

            Dispatcher.CurrentDispatcher.InvokeAsync(() =>
            {
                _vm.PieSeries.Clear();
                _vm.PieSeries.AddRange(series);
            }, DispatcherPriority.Input);
        }


        private void UpdateDates()
        {
            _vm.MinDate = _startDate.AddDays(_vm.LowerDate - _vm.MinDays);
            _vm.MaxDate = _endDate.AddDays(-(_vm.MaxDays - _vm.UpperDate));
            UpdateChart();
        }

        private void VmOnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(OverallStatsViewModel.LowerDate):
                    UpdateDates();
                    return;
                case nameof(OverallStatsViewModel.UpperDate):
                    UpdateDates();
                    return;
            }
        }

        public void Init(OverallStatsViewModel vm)
        {
            _vm = vm;
            _workTimes = _repository.FindAll(_authenticationService.User, null, null);

            if (_workTimes.Count > 0)
            {
                SetupDateSlider();
            }

            _vm.PropertyChanged += VmOnPropertyChanged;
            UpdateChart();
        }
    }
}