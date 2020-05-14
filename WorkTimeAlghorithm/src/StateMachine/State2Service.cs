using System;
using Domain.Services;
using Domain.WorkTimeAggregate;
using Serilog;
using StateMachineLib;
using System.Threading;
using System.Threading.Tasks;

namespace WMAlghorithm.StateMachine
{
    internal class State2Configuration
    {
        public int[] RetryDelays { get; set; } = new[] {4000, 15_000, 16_000, 17_000, 20_000};
        public int FaceDetectionDelayThreshold { get; set; } = 17_000;
    }

    public partial class WMonitorAlghorithm
    {
        internal class State2Service
        {
            private readonly AlghorithmFaceRecognition _faceRecognition;
            private readonly WorkTimeEventService _workTimeEventService;
            private readonly State2Configuration _config;
            private CancellationTokenSource? _cts;

            public State2Service(AlghorithmFaceRecognition faceRecognition, WorkTimeEventService workTimeEventService,
                IConfigurationService configurationService)
            {
                _faceRecognition = faceRecognition;
                _workTimeEventService = workTimeEventService;
                _config = configurationService.Get<State2Configuration>("state2");
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
                finally
                {
                    InProgress = false;
                }

                return true;
            }


            public async Task Enter(StateMachine<Triggers, States> sm, WorkTime workTime, WMonitorAlghorithm alghorithm)
            {
                _cts = new CancellationTokenSource();
                InProgress = true;

                alghorithm._canCapureMouseKeyboard = true;

                _workTimeEventService.StartTempChanges();
                _workTimeEventService.TryStartWatchingScreen();
                _workTimeEventService.StartRecognitionFailure();

                bool faceDetected = false, faceRecognized = false;

                foreach (var timeMs in _config.RetryDelays)
                {
                    (faceDetected, faceRecognized) =
                        await _faceRecognition.RecognizeFace(workTime.User).ConfigureAwait(false);
                    alghorithm.State2Result?.Invoke((faceDetected, faceRecognized));

                    if (!faceDetected && timeMs >= _config.FaceDetectionDelayThreshold)
                    {
                        alghorithm._canCapureMouseKeyboard = false;
                        _workTimeEventService.DiscardTempChanges();
                        if (!TryAddRecognitionFailure(false, faceRecognized))
                        {
                            sm.Next(Triggers.Stop);
                            return;
                        }
                        sm.Next(Triggers.NoFace);
                        return;
                    }

                    if (faceRecognized && faceDetected)
                    {
                        _workTimeEventService.StopRecognitionFailure();
                        _workTimeEventService.TryAddWatchingScreen();
                        _workTimeEventService.CommitTempChanges();
                        InProgress = false;
                        sm.Next(Triggers.FaceRecog);
                        return;
                    }


                    Log.Logger.Debug($"Starting {timeMs} state 2 delay");
                    try
                    {
                        await Task.Delay(timeMs, _cts.Token).ConfigureAwait(false);
                    }
                    catch (TaskCanceledException)
                    {
                        InProgress = false;
                        return;
                    }
                }

                _workTimeEventService.DiscardTempChanges();
                if (!TryAddRecognitionFailure(faceDetected, faceRecognized))
                {
                    sm.Next(Triggers.Stop);
                    return;
                }
                sm.Next(Triggers.FaceNotRecog);
            }
        }
    }
}