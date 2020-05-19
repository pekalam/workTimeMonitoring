using System;
using System.Threading.Tasks;
using Domain.Repositories;
using Domain.User;
using Domain.WorkTimeAggregate;
using WMAlghorithm.StateMachine;

namespace WMAlghorithm.Services
{
    public interface IAlghorithmNotificationsPort
    {
        void OnAlgorithmStopped();
        void AlghorithmOnState2Result((bool faceDetected, bool faceRecognized) args);
        void AlghorithmOnState3Result((bool faceDetected, bool faceRecognized) args);
        void Reset();
        void OnRestored(WorkTime workTime);
    }


    public class AlgorithmService
    {
        private readonly IAuthenticationService _authenticationService;
        private readonly WorkTimeBuildService _buildService;
        private readonly WMonitorAlghorithm _alghorithm;
        private readonly WorkTimeRestoreService _restoreService;
        private readonly IWorkTimeEsRepository _repository;
        private readonly IAlghorithmNotificationsPort _alghorithmNotifications;
        private bool _algStopped = false;

        public AlgorithmService(IAuthenticationService authenticationService, WorkTimeBuildService buildService,
            WMonitorAlghorithm alghorithm, IWorkTimeEsRepository repository, WorkTimeRestoreService restoreService,
            IAlghorithmNotificationsPort alghorithmNotifications)
        {
            _authenticationService = authenticationService;
            _buildService = buildService;
            _alghorithm = alghorithm;
            _repository = repository;
            _restoreService = restoreService;
            _alghorithmNotifications = alghorithmNotifications;

            _alghorithm.State3Result += arg =>
            {
                _alghorithmNotifications.AlghorithmOnState3Result(arg);
                if (arg.faceDetected && arg.faceRecognized)
                {
                    _alghorithmNotifications.Reset();
                }
            };
            _alghorithm.State2Result += arg =>
            {
                _alghorithmNotifications.AlghorithmOnState2Result(arg);
                if (arg.faceDetected && arg.faceRecognized)
                {
                    _alghorithmNotifications.Reset();
                }
            };
            _alghorithm.AlgorithmStopped += _alghorithmNotifications.OnAlgorithmStopped;
            _alghorithm.ManualRecogSuccess += () => _alghorithmNotifications.Reset();
            _alghorithm.AlgorithmStopped += () => _algStopped = true;
            _alghorithm.StopInvoked += AlgStopInvoked;
        }

        public WorkTime? CurrentWorkTime { get; private set; }

        public WMonitorAlghorithm Alghorithm => _alghorithm;

        public void StartNew(DateTime? start, DateTime end)
        {
            var created = _buildService.CreateStartedManually(_authenticationService.User, end, true);
            StartAlgorithm(created);
        }

        private void StartAlgorithm(WorkTime workTime)
        {
            _algStopped = false;
            CurrentWorkTime = workTime;
            _alghorithm.SetWorkTime(workTime);
            _alghorithm.Start();
        }

        public async Task Pause()
        {
            await _alghorithm.Pause();
            CurrentWorkTime.Pause();
        }

        public void Resume()
        {
            CurrentWorkTime.Resume();
            _alghorithm.Resume();
        }

        public async Task Stop()
        {
            await _alghorithm.Stop();
        }

        private void AlgStopInvoked()
        {
            CurrentWorkTime.Stop();
            _alghorithmNotifications.Reset();
            _repository.Save(CurrentWorkTime);
            CurrentWorkTime.MarkPendingEventsAsHandled();
        }

        public bool TryRestore()
        {
            var user = _authenticationService.User;
            if (user != null && _restoreService.Restore(user, out var restored))
            {
                StartAlgorithm(restored);
                _alghorithmNotifications.OnRestored(restored);
                return true;
            }

            return false;
        }

        public async void Shutdown()
        {
            if (CurrentWorkTime == null)
            {
                return;
            }
            if (!_algStopped && CurrentWorkTime.Stopped)
            {
                await _alghorithm.Stop();
            }
            else if(!CurrentWorkTime.Stopped)
            {
                _restoreService.SetInterrupted(CurrentWorkTime);
            }
        }
    }
}