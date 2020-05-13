using Prism.Commands;
using Prism.Events;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UI.Common.Messaging;
using WindowUI.Messaging;
using WMAlghorithm.Services;

namespace WindowUI.StartWork
{
    public interface IStartWorkViewController
    {
        DelegateCommand StartWork { get; }
        DelegateCommand StopWork { get; }
        DelegateCommand TogglePauseWork { get; }
        void OnTimerStopped();
    }

    public class StartWorkViewController : IStartWorkViewController, IDisposable
    {
        private StartWorkViewModel _vm;
        private SubscriptionToken _monRestoredToken;
        private SubscriptionToken _windowRestoredToken;
        private readonly IEventAggregator _ea;
        private readonly AlgorithmService _algorithmService;
        private bool _stopRequested = false;
        private bool _pauseRequested = false;


        public StartWorkViewController(AlgorithmService algorithmService, IEventAggregator ea)
        {
            _algorithmService = algorithmService;
            _ea = ea;
            StartWork = new DelegateCommand(OnStartWorkExecute);
            StopWork = new DelegateCommand(OnStopWorkExecute,
                () => !_pauseRequested && !_stopRequested && (!_vm?.IsPaused ?? true));
            TogglePauseWork = new DelegateCommand(TogglePauseExecute, () => !_pauseRequested && !_stopRequested);
        }

        private void RaiseCanExecChanged()
        {
            StopWork.RaiseCanExecuteChanged();
            TogglePauseWork.RaiseCanExecuteChanged();
        }

        private async void TogglePauseExecute()
        {
            _pauseRequested = true;
            RaiseCanExecChanged();
            if (!_vm.IsPaused)
            {
                _algorithmService.Resume();
            }
            else
            {
                await _algorithmService.Pause();
            }

            _pauseRequested = false;
            RaiseCanExecChanged();
        }

        private async void OnStopWorkExecute()
        {
            if (_vm.IsPaused)
            {
                _algorithmService.Resume();
            }

            _stopRequested = true;
            RaiseCanExecChanged();
            await _algorithmService.Stop();
            _vm.Started = _stopRequested = false;
            RaiseCanExecChanged();
        }

        private void OnStartWorkExecute()
        {
            if (_vm.HasErrors)
            {
                _vm.Validate();
                return;
            }

            if (_vm.AutoStart && _vm.StartDate < DateTime.Now)
            {
                _vm.StartDate = DateTime.Now;
            }

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

            _algorithmService.StartNew(start, end.Value);
            _vm.Started = true;
        }

        private void SetAlgorithmStarted()
        {
            _vm.EndDate = _algorithmService.CurrentWorkTime?.EndDate.ToLocalTime();
            _vm.Started = true;
        }

        public void Init(StartWorkViewModel vm)
        {
            _vm = vm;
            _ea.GetEvent<MonitoringRestored>().Subscribe(_ => SetAlgorithmStarted());
            _ea.GetEvent<WindowRestored>().Subscribe(_ =>
            {
                if (_algorithmService.CurrentWorkTime != null && _vm.TimerDate.TotalMilliseconds > 0)
                {
                    _vm.SetTimerDate(_algorithmService.CurrentWorkTime.EndDate.ToLocalTime());
                }
            });
        }

        public async void OnTimerStopped()
        {
            await _algorithmService.Stop();
        }

        public void Dispose()
        {
            _ea.GetEvent<MonitoringRestored>().Unsubscribe(_monRestoredToken);
            _ea.GetEvent<WindowRestored>().Unsubscribe(_windowRestoredToken);
        }

        public DelegateCommand StartWork { get; private set; }
        public DelegateCommand StopWork { get; private set; }
        public DelegateCommand TogglePauseWork { get; private set; }
    }
}