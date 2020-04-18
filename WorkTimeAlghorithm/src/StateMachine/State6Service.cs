using System.Threading.Tasks;
using StateMachineLib;

namespace WorkTimeAlghorithm.StateMachine
{
    internal class State6Service
    {
        public async Task Enter(WMonitorAlghorithm.State state,
            StateMachine<WMonitorAlghorithm.Triggers, WMonitorAlghorithm.States> sm)
        {
            await Task.Delay(60_000 * 5);

            sm.Next(WMonitorAlghorithm.Triggers.FaceNotRecog);
        }
    }
}