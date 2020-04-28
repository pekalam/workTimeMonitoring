using Domain.Repositories;
using Domain.User;
using Prism.Events;
using Unity;
using WindowUI.RepoProxy;

namespace WindowUI.Statistics
{

    public class StatisticsViewController
    {
        private StatisticsViewModel _vm;
        private IAuthenticationService _authenticationService;
        private IWorkTimeEsRepository _repository;
        private readonly OverallStatsController _overallStatsController;
        private readonly DailyStatsViewController _dailyStatsViewController;

        public StatisticsViewController(IAuthenticationService authenticationService, [Dependency(nameof(WorkTimeEsRepositorDecorator))] IWorkTimeEsRepository repository, OverallStatsController overallStatsController, DailyStatsViewController dailyStatsViewController)
        {
            _authenticationService = authenticationService;
            _repository = repository;
            _overallStatsController = overallStatsController;
            _dailyStatsViewController = dailyStatsViewController;
        }

        public void Init(StatisticsViewModel vm)
        {
            _vm = vm;
            _vm.OverallStatsViewModel.Controller = _overallStatsController;
            _vm.DailyStatsViewModel.Controller = _dailyStatsViewController;
            _vm.TabChanged += VmOnTabChanged;
        }

        private void VmOnTabChanged(int index)
        {
            if (index == 0)
            {
                _overallStatsController.UpdateChart();
            }else if (index == 1)
            {
                _dailyStatsViewController.UpdateChart();
            }
        }
    }
}