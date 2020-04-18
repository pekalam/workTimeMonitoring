using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using StateMachineLib;

namespace WorkTimeAlghorithm.StateMachine
{
    internal class State5Service
    {
        private CancellationTokenSource _cts;

        public async Task Enter(WMonitorAlghorithm.State state,
            StateMachine<WMonitorAlghorithm.Triggers, WMonitorAlghorithm.States> sm)
        {
            state.CanCapureMouseKeyboard = true;

            _cts = new CancellationTokenSource();

            var t= Task.Delay(120_000, _cts.Token);

            try
            {
                await t;
                Debug.WriteLine("State 5 check face timeout");
                sm.Next(WMonitorAlghorithm.Triggers.FaceNotRecog);
            }
            catch (TaskCanceledException e)
            {
                Debug.WriteLine("State 5 task cancelled");
            }
        }

        public void Exit()
        {
            _cts.Cancel();
        }
    }
}