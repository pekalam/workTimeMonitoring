using Domain.Repositories;
using Domain.User;
using Prism.Events;

namespace WindowUI.Statistics
{

    public class StatisticsViewController
    {
        private StatisticsViewModel _vm;
        private IAuthenticationService _authenticationService;
        private IWorkTimeEsRepository _repository;

        public StatisticsViewController(IAuthenticationService authenticationService, IWorkTimeEsRepository repository)
        {
            _authenticationService = authenticationService;
            _repository = repository;
        }

        public void Init(StatisticsViewModel vm)
        {
            _vm = vm;
        }
    }
}