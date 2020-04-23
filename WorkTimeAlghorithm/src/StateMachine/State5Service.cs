using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Serilog;
using StateMachineLib;

namespace WorkTimeAlghorithm.StateMachine
{
    internal class State5Service
    {
        private int GetRandomDelay()
        {
            var rnd = new Random();
            return rnd.Next(50_000, 120_000);
        }

        public async Task Enter(WMonitorAlghorithm.State state,
            StateMachine<WMonitorAlghorithm.Triggers, WMonitorAlghorithm.States> sm)
        {
            state.CanCapureMouseKeyboard = true;

            int timeMs = GetRandomDelay();

            Log.Logger.Debug($"Starting state 5 delay {timeMs}");
            await Task.Delay(timeMs);

            Log.Logger.Debug("State 5 face compare timeout");
            sm.Next(WMonitorAlghorithm.Triggers.FaceNotRecog);
        }

        public void Exit()
        {
        }
    }
}