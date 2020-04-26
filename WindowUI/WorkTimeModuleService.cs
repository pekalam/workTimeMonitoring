using System;
using System.Collections.Generic;
using System.Text;
using Domain.Repositories;
using Domain.User;
using Domain.WorkTimeAggregate;
using Infrastructure.Messaging;
using Prism.Events;
using WorkTimeAlghorithm.StateMachine;

namespace WindowUI
{
    public class WorkTimeModuleService
    {
        private readonly IAuthenticationService _authenticationService;
        private readonly WorkTimeBuildService _buildService;
        private readonly WMonitorAlghorithm _alghorithm;
        private readonly WorkTimeRestoreService _restoreService;
        private readonly IWorkTimeEsRepository _repository;

        public WorkTimeModuleService(IAuthenticationService authenticationService, WorkTimeBuildService buildService,
            WMonitorAlghorithm alghorithm, IWorkTimeEsRepository repository, WorkTimeRestoreService restoreService
            ,IEventAggregator ea)
        {
            _authenticationService = authenticationService;
            _buildService = buildService;
            _alghorithm = alghorithm;
            _repository = repository;
            _restoreService = restoreService;
            ea.GetEvent<AppShuttingDownEvent>().Subscribe(() => _restoreService.SetInterrupted(CurrentWorkTime), true);
        }

        public bool AlgorithmStarted { get; private set; }
        public WorkTime CurrentWorkTime { get; private set; }

        public void StartNew(DateTime? start, DateTime end)
        {
            var created = _buildService.CreateStartedManually(_authenticationService.User, end, true);
            StartAlgorithm(created);
        }

        private void StartAlgorithm(WorkTime workTime)
        {
            CurrentWorkTime = workTime;
            _alghorithm.SetWorkTime(workTime);
            _alghorithm.Start();
            AlgorithmStarted = true;
        }

        public void Pause()
        {
            CurrentWorkTime.Pause();
            _alghorithm.Pause();
        }

        public void Resume()
        {
            CurrentWorkTime.Resume();
            _alghorithm.Resume();
        }

        public void Stop()
        {
            CurrentWorkTime.Stop();
            _alghorithm.Stop();
        }

        public bool TryRestore()
        {
            var user = _authenticationService.User;
            if (_restoreService.Restore(user, out var restored))
            {
                StartAlgorithm(restored);
                return true;
            }

            return false;
        }
    }
}