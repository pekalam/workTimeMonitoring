using Serilog;
using StateMachineLib;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace WMAlghorithm.StateMachine
{
    public partial class WMonitorAlghorithm
    {
        internal class State5Service
        {
            private CancellationTokenSource _cts;

            private int GetRandomDelay()
            {
                var rnd = new Random();
                return rnd.Next(5_000, 6_000);
                //return rnd.Next(50_000, 120_000);
            }

            public bool InProgress { get; private set; }
            public void Cancel() => _cts?.Cancel();

            public async Task Enter(WMonitorAlghorithm alghorithm, StateMachine<Triggers, States> sm)
            {
                InProgress = true;
                _cts = new CancellationTokenSource();
                alghorithm._workTimeEventService.TryStartWatchingScreen();

                alghorithm._canCapureMouseKeyboard = true;

                int timeMs = GetRandomDelay();

                Log.Logger.Debug($"Starting state 5 delay {timeMs}");
                try
                {
                    await Task.Delay(timeMs, _cts.Token);
                }
                catch (TaskCanceledException)
                {
                    InProgress = false;
                    return;
                }

                alghorithm._workTimeEventService.TryAddWatchingScreen();

                InProgress = false;
                Log.Logger.Debug("State 5 face compare timeout");
                sm.Next(WMonitorAlghorithm.Triggers.FaceNotRecog);
            }
        }
    }
}