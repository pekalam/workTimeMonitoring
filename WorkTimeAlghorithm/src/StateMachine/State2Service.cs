using System.Threading.Tasks;
using Domain.Services;
using Serilog;
using StateMachineLib;

namespace WorkTimeAlghorithm.StateMachine
{
    internal class State2Service
    {
        private readonly AlghorithmFaceRecognition _faceRecognition;
        private readonly WorkTimeEventService _workTimeEventService;

        public State2Service(AlghorithmFaceRecognition faceRecognition, WorkTimeEventService workTimeEventService)
        {
            _faceRecognition = faceRecognition;
            _workTimeEventService = workTimeEventService;
        }

        public async Task Enter(WMonitorAlghorithm.State state, StateMachine<WMonitorAlghorithm.Triggers, WMonitorAlghorithm.States> sm)
        {
            state.CanCapureMouseKeyboard = true;

            _workTimeEventService.StartTempChanges();

            foreach (var timeMs in new[] { 4000, 6000, 8000, 12_000, 30_000 })
            {

                var (faceDetected, faceRecognized) = await _faceRecognition.RecognizeFace();


                if (!faceDetected && timeMs == 12_000)
                {
                    sm.Next(WMonitorAlghorithm.Triggers.NoFace);
                    return;
                }

                if (faceRecognized && faceDetected)
                {
                    sm.Next(WMonitorAlghorithm.Triggers.FaceRecog);
                    return;
                }


                Log.Logger.Debug($"Starting {timeMs} delay");
                await Task.Delay(timeMs);
            }

            sm.Next(WMonitorAlghorithm.Triggers.FaceNotRecog);
        }

        public void Exit(WMonitorAlghorithm.Triggers trigger)
        {
            if (trigger == WMonitorAlghorithm.Triggers.FaceRecog)
            {
                _workTimeEventService.CommitTempChanges();
            }
        }
    }
}