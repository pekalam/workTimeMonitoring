using System;
using System.Collections.Generic;
using System.Text;
using Domain.Repositories;
using Domain.User;
using Domain.WorkTimeAggregate;
using WorkTimeAlghorithm.StateMachine;

namespace WindowUI
{
    public class WorkTimeModuleService
    {
        private WorkTime _currentWorkTime;
        private readonly IAuthenticationService _authenticationService;
        private readonly WorkTimeBuildService _workTimeBuildService;
        private readonly WMonitorAlghorithm _alghorithm;
        private readonly IWorkTimeEsRepository _workTimeRepository;

        public WorkTimeModuleService(IAuthenticationService authenticationService, WorkTimeBuildService workTimeBuildService, WMonitorAlghorithm alghorithm, IWorkTimeEsRepository workTimeRepository)
        {
            _authenticationService = authenticationService;
            _workTimeBuildService = workTimeBuildService;
            _alghorithm = alghorithm;
            _workTimeRepository = workTimeRepository;
        }

        public bool AlgorithmStarted { get; private set; }

        public void StartNew(DateTime? start, DateTime end)
        {
            var created = _workTimeBuildService.CreateStartedManually(_authenticationService.User, end, true);
            StartAlgorithm(created);
        }

        private void StartAlgorithm(WorkTime workTime)
        {
            _currentWorkTime = workTime;
            _alghorithm.SetWorkTime(workTime);
            _alghorithm.Start();
            AlgorithmStarted = true;
        }

        public void Pause()
        {
            throw new NotImplementedException();
        }

        public void Stop()
        {
            throw new NotImplementedException();
        }

        public bool TryRestore()
        {
            var user = _authenticationService.User;
            var restoredWorkTime = _workTimeRepository.FindLatestFromSnapshot(user);
            if (restoredWorkTime != null)
            {
                if (restoredWorkTime.EndDate < DateTime.UtcNow)
                {
                    StartAlgorithm(restoredWorkTime);
                    return true;
                }
            }
            return false;
        }
    }
}
