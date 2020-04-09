using System.Threading;
using StateMachineLib;

namespace Infrastructure.WorkTime
{
    public partial class WMonitorAlghorithm
    {
        public enum Triggers
        {
            Start
        }

        public enum States
        {
            s0, FaceInit
        }

        private StateMachine<Triggers, States> _sm;
#if DEBUG
        private StateMachineVis<Triggers, States> _vis;
#endif

        private void InitStateMachine()
        {
            _sm = new StateMachineBuilder<Triggers, States>()
                .CreateState(States.s0)

                .Transition(Triggers.Start, States.FaceInit)

                .End()


                .CreateState(States.FaceInit)
                .Enter(async _ =>
                {
                    // await _initFaceService.InitFace(new CancellationTokenSource()).ConfigureAwait(false);
                })
                .End()

                .Build(States.s0);

#if DEBUG
            _vis = new StateMachineVis<Triggers, States>(_sm, pipeName: "graphViz", loggingEnabled: false);
            _vis.Start(@"StateMachineLibVis.exe", "-c graphViz -l 970");
#endif
        }

    }
}