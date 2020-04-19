using System;
using StateMachineLib;

namespace WorkTimeAlghorithm.StateMachine
{
    public partial class WMonitorAlghorithm
    {
        internal enum Triggers
        {
            Start, NoFace, FaceNotRecog, FaceRecog, MouseMv, KeyboardMv
        }

        internal enum States
        {
            s1,s2,s3,s5,s6
        }

        internal class State
        {
            public bool CanCapureMouseKeyboard;
        }

        private StateMachine<Triggers, States> _sm;
#if DEBUG
        private StateMachineVis<Triggers, States> _vis;
#endif
        private State _state = new State();

        private async void State2Action(Triggers t)
        {
            await _state2.Enter(_state, _sm, _workTime);
        }

        private async void State3Action(Triggers t)
        {
            await _state3.Enter(_state, _sm, _workTime);
        }


        private void BuildStateMachine()
        {
            _sm = new StateMachineBuilder<Triggers, States>()
                .CreateState(States.s1)
                .Ignoring()
                .Transition(Triggers.Start, States.s2)
                .End()

                .CreateState(States.s2)
                .Enter(State2Action)
                .Exit(t => _state2.Exit(t))
                .Transition(Triggers.FaceRecog, States.s5)
                .Transition(Triggers.FaceNotRecog, States.s3)
                .Transition(Triggers.NoFace, States.s3)
                .Ignoring()
                .End()

                .CreateState(States.s3)
                .Ignoring()
                .Enter(State3Action)
                .Transition(Triggers.FaceRecog, States.s5)
                .End()

                .CreateState(States.s5)
                .Ignoring()
                .Transition(Triggers.MouseMv, States.s6)
                .Transition(Triggers.KeyboardMv, States.s6)
                .Transition(Triggers.FaceNotRecog, States.s2)
                .Enter(async t => await _state5.Enter(_state, _sm))
                .Exit(t => _state5.Exit())
                .End()
                
                .CreateState(States.s6)
                .Ignoring()
                .Enter(async t => await _state6.Enter(_state, _sm))
                .End()

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