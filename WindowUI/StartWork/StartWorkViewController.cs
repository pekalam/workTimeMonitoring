using System;
using Prism.Commands;

namespace WindowUI.StartWork
{
    public interface IStartWorkViewController
    {
        DelegateCommand StartWork { get; }
        public DelegateCommand StopWork { get; }
        public DelegateCommand PauseWork { get; }
        public DelegateCommand ResumeWork { get; }
    }

    public class StartWorkViewController : IStartWorkViewController
    {
        private StartWorkViewModel _vm;
        private readonly WorkTimeModuleService _workTimeModuleService;

        public StartWorkViewController(WorkTimeModuleService workTimeModuleService)
        {
            _workTimeModuleService = workTimeModuleService;
            StartWork = new DelegateCommand(OnStartWorkExecute, CanExecuteMethod);
            StopWork = new DelegateCommand(OnStopWorkExecute);
            PauseWork = new DelegateCommand(OnPauseWorkExecute);
            ResumeWork = new DelegateCommand(OnResumeWorkExecute);
        }

        private void OnResumeWorkExecute()
        {
            _workTimeModuleService.Resume();
        }

        private void OnPauseWorkExecute()
        {
            _workTimeModuleService.Pause();
        }

        private void OnStopWorkExecute()
        {
            _workTimeModuleService.Stop();
            _vm.Started = false;
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

        public DelegateCommand StartWork { get; private set; }
        public DelegateCommand StopWork { get; private set; }
        public DelegateCommand PauseWork { get; private set; }
        public DelegateCommand ResumeWork { get; private set; }
    }
}