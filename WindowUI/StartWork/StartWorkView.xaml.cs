using System;
using System.Globalization;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Threading;
using Prism.Commands;
using Prism.Mvvm;
using Prism.Regions;

namespace WindowUI.StartWork
{
    /// <summary>
    /// Interaction logic for StartWorkView
    /// </summary>
    public partial class StartWorkView : UserControl
    {
        public StartWorkView()
        {
            InitializeComponent();
        }
    }

    public class StartWorkViewModel : BindableBase, INavigationAware
    {
        private readonly StartWorkViewController _controller;
        private bool _autoStart;
        private bool _started = true;
        private DateTime? _startDate = DateTime.Now;
        private DateTime? _endDate = DateTime.Now;
        private readonly DispatcherTimer _timer = new DispatcherTimer();
        private TimeSpan _timerDate;

        public StartWorkViewModel(StartWorkViewController controller)
        {
            _controller = controller;
            StartWorkCommand = controller.StartWorkCommand;
            _timer.Tick += TimerOnTick;
            _timer.Interval = TimeSpan.FromSeconds(1);
        }

        private void TimerOnTick(object? sender, EventArgs e)
        {
            TimerDate = TimerDate.Subtract(TimeSpan.FromSeconds(1));
        }

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
                TimerDate = endDate.Subtract(DateTime.Now);
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
            set => SetProperty(ref _timerDate, value);
        }

        public DelegateCommand StartWorkCommand { get; set; }

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

    public class StartWorkViewController
    {
        private StartWorkViewModel _vm;
        private readonly WorkTimeModuleService _workTimeModuleService;

        public StartWorkViewController(WorkTimeModuleService workTimeModuleService)
        {
            _workTimeModuleService = workTimeModuleService;
            StartWorkCommand = new DelegateCommand(OnStartWorkExecute, CanExecuteMethod);
        }

        private bool CanExecuteMethod()
        {
            return true;
        }

        private void OnStartWorkExecute()
        {
            DateTime? start;
            if (_vm.AutoStart)
            {
                start = null;
            }
            else
            {
                start = _vm.StartDate?.ToUniversalTime();
            }
            DateTime? end = _vm.EndDate?.ToUniversalTime();

            _workTimeModuleService.StartNew(start, end.Value);
            _vm.Started = true;
        }

        public void Init(StartWorkViewModel vm)
        {
            _vm = vm;
            _vm.Started = _workTimeModuleService.AlgorithmStarted;
            if (_workTimeModuleService.AlgorithmStarted)
            {
                _vm.SetTimerDate(_workTimeModuleService.CurrentWorkTime.EndDate.ToLocalTime());
            }
        }

        public DelegateCommand StartWorkCommand { get; private set; }
    }

    public class TimerConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var date = (TimeSpan) value;

            string h = date.Hours < 10 ? "0" +date.Hours : date.Hours.ToString();
            string m = date.Minutes < 10 ? "0" + date.Minutes : date.Minutes.ToString();
            string s = date.Seconds < 10 ? "0" + date.Seconds: date.Seconds.ToString();
            return $"{h}:{m}:{s}";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
