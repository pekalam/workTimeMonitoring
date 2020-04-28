using System;
using System.Threading.Tasks;
using Domain.Services;
using Domain.WorkTimeAggregate;
using Serilog;
using StateMachineLib;

namespace WorkTimeAlghorithm.StateMachine
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

            public State3Service(AlghorithmFaceRecognition faceRecognition, WorkTimeEventService workTimeEventService,
                IConfigurationService configurationService)
            {
                _faceRecognition = faceRecognition;
                _workTimeEventService = workTimeEventService;
                _config = configurationService.Get<State3Configuration>("state3");
            }


            public async Task Enter(State state,
                StateMachine<Triggers, States> sm, WorkTime workTime, WMonitorAlghorithm alghorithm)
            {
                state.CanCapureMouseKeyboard = true;
                bool faceDetected = false;
                bool faceRecognized = false;


                while (!faceRecognized || !faceDetected)
                {
                    _workTimeEventService.StartTempChanges();
                    _workTimeEventService.StartRecognitionFailure();

                    Log.Logger.Debug($"Starting {_config.Delay} state 3 delay");

                    await Task.Delay(_config.Delay);

                    (faceDetected, faceRecognized) = await _faceRecognition.RecognizeFace(workTime.User);
                    alghorithm.State3Result((faceDetected, faceRecognized));

                    Log.Logger.Debug(
                        $"State 3 face recognized: {faceRecognized} face detected face recognized: {faceDetected}");

                    if (!faceRecognized || !faceDetected)
                    {
                        _workTimeEventService.DiscardTempChanges();
                        _workTimeEventService.AddRecognitionFailure(faceDetected, faceRecognized);
                    }
                    else
                    {
                        _workTimeEventService.StopRecognitionFailure();
                        _workTimeEventService.CommitTempChanges();
                    }
                }

                sm.Next(WMonitorAlghorithm.Triggers.FaceRecog);
            }
        }
    }
}