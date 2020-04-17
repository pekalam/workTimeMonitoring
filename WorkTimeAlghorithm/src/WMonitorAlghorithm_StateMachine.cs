using System.Threading.Tasks;
using Serilog;
using StateMachineLib;

namespace WorkTimeAlghorithm
{
    public partial class WMonitorAlghorithm
    {
        public enum Triggers
        {
            Start, NoFace, FaceNotRecog, FaceRecog, MouseMv, KeyboardMv
        }

        public enum States
        {
            s1,s2,s3,s5,s6
        }

        private StateMachine<Triggers, States> _sm;
#if DEBUG
        private StateMachineVis<Triggers, States> _vis;
#endif

        private async void State2Action(Triggers t)
        {
            foreach(var timeMs in new[] { 4000, 6000, 8000, 12_000, 30_000})
            {

                var (faceDetected, faceRecognized) = await RecognizeFace();


                if (!faceDetected && timeMs == 12_000)
                {
                    _sm.Next(Triggers.NoFace);
                    return;
                }

                if (faceRecognized)
                {
                    _sm.Next(Triggers.FaceRecog);
                    return;
                }


                Log.Logger.Debug($"Starting {timeMs} delay");
                await Task.Delay(timeMs);
            }

            _sm.Next(Triggers.FaceNotRecog);
        }

        private async void State3Action(Triggers t)
        {
            int timeMs = 10_000;
            bool faceRecognized = false;

            while (!faceRecognized)
            {
                Log.Logger.Debug($"Starting {timeMs} state 3 delay");
                await Task.Delay(timeMs);
                (_, faceRecognized) = await RecognizeFace();
            }

            _sm.Next(Triggers.FaceRecog);
            return;
        }


        private void BuildStateMachine()
        {
            _sm = new StateMachineBuilder<Triggers, States>()
                .CreateState(States.s1)
                .Transition(Triggers.Start, States.s2)
                .End()

                .CreateState(States.s2)
                .Enter(State2Action)
                .Transition(Triggers.FaceRecog, States.s5)
                .Transition(Triggers.FaceNotRecog, States.s3)
                .Transition(Triggers.NoFace, States.s3)
                .End()

                .CreateState(States.s3)
                .Enter(State3Action)
                .Transition(Triggers.FaceRecog, States.s5)
                .End()

                .CreateState(States.s5).End().CreateState(States.s6).End()

                .Build(States.s1);
        }


        private void InitStateMachine()
        {
            BuildStateMachine();

#if DEBUG
            _vis = new StateMachineVis<Triggers, States>(_sm, pipeName: "graphViz", loggingEnabled: false);
            _vis.Start(@"StateMachineLibVis.exe", "-c graphViz -l 970");
#endif
        }

    }
}