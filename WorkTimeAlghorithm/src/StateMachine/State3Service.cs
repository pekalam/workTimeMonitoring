using Domain.Services;
using Domain.WorkTimeAggregate;
using Serilog;
using StateMachineLib;
using System.Threading;
using System.Threading.Tasks;

namespace WMAlghorithm.StateMachine
{
    internal class State3Configuration
    {
        public int Delay { get; set; } = 30_000;
    }

    public partial class WMonitorAlghorithm
    {
        internal class State3Service
        {
            private readonly AlghorithmFaceRecognition _faceRecognition;
            private readonly WorkTimeEventService _workTimeEventService;
            private readonly State3Configuration _config;
            private CancellationTokenSource? _cts;

            public State3Service(AlghorithmFaceRecognition faceRecognition, WorkTimeEventService workTimeEventService,
                IConfigurationService configurationService)
            {
                _faceRecognition = faceRecognition;
                _workTimeEventService = workTimeEventService;
                _config = configurationService.Get<State3Configuration>("state3");
            }

            public bool InProgress { get; private set; }
            public void Cancel() => _cts?.Cancel();


            private bool TryAddRecognitionFailure(bool faceDetected, bool faceRecognized)
            {
                try
                {
                    _workTimeEventService.AddRecognitionFailure(faceDetected, faceRecognized);
                }
                catch (WorkTimeStoppedException)
                {
                    return false;
                }

                return true;
            }

            public async Task Enter(StateMachine<Triggers, States> sm, WorkTime workTime, WMonitorAlghorithm alghorithm)
            {
                InProgress = true;
                _cts = new CancellationTokenSource();
                alghorithm._canCapureMouseKeyboard = true;
                bool faceDetected = false;
                bool faceRecognized = false;


                while (!faceRecognized || !faceDetected)
                {
                    _workTimeEventService.StartTempChanges();
                    _workTimeEventService.TryStartWatchingScreen();
                    _workTimeEventService.StartRecognitionFailure();

                    Log.Logger.Debug($"Starting {_config.Delay} state 3 delay");

                    try
                    {
                        await Task.Delay(_config.Delay, _cts.Token);
                    }
                    catch (TaskCanceledException)
                    {
                        InProgress = false;
                        return;
                    }

                    (faceDetected, faceRecognized) = await _faceRecognition.RecognizeFace(workTime.User);
                    alghorithm.State3Result((faceDetected, faceRecognized));

                    Log.Logger.Debug(
                        $"State 3 face recognized: {faceRecognized} face detected face recognized: {faceDetected}");

                    if (!faceRecognized || !faceDetected)
                    {
                        _workTimeEventService.DiscardTempChanges();
                        if (!TryAddRecognitionFailure(faceDetected, faceRecognized))
                        {
                            InProgress = false;
                            sm.Next(Triggers.Stop);
                            return;
                        }
                    }
                    else
                    {
                        _workTimeEventService.TryAddWatchingScreen();
                        _workTimeEventService.StopRecognitionFailure();
                        _workTimeEventService.CommitTempChanges();
                    }

                }

                InProgress = false;
                sm.Next(Triggers.FaceRecog);
            }
        }
    }
}