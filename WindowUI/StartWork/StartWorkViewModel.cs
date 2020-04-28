using System;
using System.Windows.Threading;
using Prism.Commands;
using Prism.Mvvm;
using Prism.Regions;

namespace WindowUI.StartWork
{
    public class StartWorkViewModel : BindableBase, INavigationAware
    {
        private readonly StartWorkViewController _controller;
        private bool _autoStart;
        private bool _started;
        private DateTime? _startDate = DateTime.Now;
        private DateTime? _endDate = DateTime.Now;
        private readonly DispatcherTimer _timer = new DispatcherTimer();
        private TimeSpan _timerDate;

        public StartWorkViewModel(StartWorkViewController controller)
        {
            _controller = controller;
            StartWork = controller.StartWork;
            StopWork = controller.StopWork;
            PauseWork = controller.PauseWork;
            ResumeWork = controller.ResumeWork;
            _timer.Tick += TimerOnTick;
            _timer.Interval = TimeSpan.FromSeconds(1);
        }

        private void TimerOnTick(object? sender, EventArgs e)
        {
            TimerDate = TimerDate.Subtract(TimeSpan.FromSeconds(1));
        }

        public DelegateCommand StartWork { get; set; }
        public DelegateCommand StopWork { get; }
        public DelegateCommand PauseWork { get; }
        public DelegateCommand ResumeWork { get; }

        public bool AutoStart
        {
            get => _autoStart;
            set => SetProperty(ref _autoStart, value);
        }

        public bool Started
        {
            get => _started;
            set
            {
                SetProperty(ref _started, value);
                if (value)
                {
                    SetTimerDate(EndDate.Value);
                    StartTimer();
                }
            }
        }

        public void SetTimerDate(DateTime endDate)
        {
            if (AutoStart)
            {
                TimerDate = endDate.Subtract(StartDate.Value);
            }
            else
            {
                TimerDate = endDate.Subtract(DateTime.UtcNow);
            }
        }

        private void StartTimer()
        {
            _timer.Start();
        }

        public DateTime? StartDate
        {
            get => _startDate;
            set => SetProperty(ref _startDate, value);
        }

        public DateTime? EndDate
        {
            get => _endDate;
            set => SetProperty(ref _endDate, value);
        }

        public TimeSpan TimerDate
        {
            get => _timerDate;
            set
            {
                SetProperty(ref _timerDate, value);
                if (value.TotalMilliseconds <= 0)
                {
                    Started = false;
                    _timer.Stop();
                }
            }
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