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

    internal class State3Service
    {
        private AlghorithmFaceRecognition _faceRecognition;
        private readonly WorkTimeEventService _workTimeEventService;
        private readonly State3Configuration _config;

        public State3Service(AlghorithmFaceRecognition faceRecognition, WorkTimeEventService workTimeEventService, IConfigurationService configurationService)
        {
            _faceRecognition = faceRecognition;
            _workTimeEventService = workTimeEventService;
            _config = configurationService.Get<State3Configuration>("state3");
        }


        public async Task Enter(WMonitorAlghorithm.State state, StateMachine<WMonitorAlghorithm.Triggers, WMonitorAlghorithm.States> sm, WorkTime workTime)
        {
            state.CanCapureMouseKeyboard = false;

            bool faceDetected = false;
            bool faceRecognized = false;

            Log.Logger.Debug("Discarding temp changes");
            _workTimeEventService.DiscardTempChanges();

            while (!faceRecognized || !faceDetected)
            {
                Log.Logger.Debug($"Starting {_config.Delay} state 3 delay");

                await Task.Delay(_config.Delay);
                (faceDetected, faceRecognized) = await _faceRecognition.RecognizeFace(workTime.User);

                Log.Logger.Debug($"State 3 face recognized: {faceRecognized} face detected face recognized: {faceDetected}");
            }

            sm.Next(WMonitorAlghorithm.Triggers.FaceRecog);
        }
    }
}