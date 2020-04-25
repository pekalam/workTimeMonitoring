using System;
using Prism.Commands;

namespace WindowUI.StartWork
{
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
}