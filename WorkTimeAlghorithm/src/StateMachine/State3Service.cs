using System.Threading.Tasks;
using Domain.Services;
using Serilog;
using StateMachineLib;

namespace WorkTimeAlghorithm.StateMachine
{
    internal class State3Service
    {
        private AlghorithmFaceRecognition _faceRecognition;
        private readonly WorkTimeEventService _workTimeEventService;

        public State3Service(AlghorithmFaceRecognition faceRecognition, WorkTimeEventService workTimeEventService)
        {
            _faceRecognition = faceRecognition;
            _workTimeEventService = workTimeEventService;
        }

        public async Task Enter(WMonitorAlghorithm.State state, StateMachine<WMonitorAlghorithm.Triggers, WMonitorAlghorithm.States> sm)
        {
            state.CanCapureMouseKeyboard = false;

            int timeMs = 10_000;
            bool faceRecognized = false;

            Log.Logger.Debug("Discarding temp changes");
            _workTimeEventService.DiscardTempChanges();

            while (!faceRecognized)
            {
                Log.Logger.Debug($"Starting {timeMs} state 3 delay");
                await Task.Delay(timeMs);
                (_, faceRecognized) = await _faceRecognition.RecognizeFace();
            }

            sm.Next(WMonitorAlghorithm.Triggers.FaceRecog);
            return;
        }
    }
}