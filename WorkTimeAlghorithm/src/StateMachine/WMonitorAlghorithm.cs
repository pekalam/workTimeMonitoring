using Domain.Services;
using Domain.WorkTimeAggregate;
using Domain.WorkTimeAggregate.Events;
using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using Serilog;

namespace WMAlghorithm.StateMachine
{
    public partial class WMonitorAlghorithm
    {
        private readonly IMouseKeyboardMonitorService _mouseKeyboardMonitor;
        private readonly WorkTimeEventService _workTimeEventService;
        private readonly State2Service _state2;
        private readonly State3Service _state3;
        private readonly State5Service _state5 = new State5Service();
        private WorkTime _workTime;
        private readonly ICaptureService _captureService;
        private bool _canCapureMouseKeyboard;
        private SemaphoreSlim _stopSem = new SemaphoreSlim(1,1);

        public event Action<(bool faceDetected, bool faceRecognized)> State3Result;
        public event Action<(bool faceDetected, bool faceRecognized)> State2Result;
        public event Action AlgorithmStopped;
        public event Action AlgorithmStarted;
        public event Action ManualRecogSuccess;
        public event Action StopInvoked; 

        private IDisposable _keyboardSub;
        private IDisposable _mouseSub;

        public WMonitorAlghorithm(AlghorithmFaceRecognition faceRecognition, WorkTimeEventService workTimeEventService,
            IConfigurationService configurationService, IMouseKeyboardMonitorService mouseKeyboardMonitor,
            ICaptureService captureService)
        {
            _mouseKeyboardMonitor = mouseKeyboardMonitor;
            _captureService = captureService;
            _workTimeEventService = workTimeEventService;
            _state2 = new State2Service(faceRecognition, workTimeEventService, configurationService);
            _state3 = new State3Service(faceRecognition, workTimeEventService, configurationService);
        }

        public bool ManualRecog => _sm.CurrentState.Name == States.MANUAL;

        public bool Started => _sm != null && _sm.CurrentState.Name != States.s1 &&
                               _sm.CurrentState.Name != States.PAUSE_STATE &&
                               _sm.CurrentState.Name != States.STOP_STATE;


        private void OnMouseStart(DateTime d)
        {
            if (_canCapureMouseKeyboard && !_workTime.Paused && !_workTime.Stopped)
            {
                _workTimeEventService.SetMkEventStart(d, true);
            }
        }

        private void OnKbdStart(DateTime d)
        {
            if (_canCapureMouseKeyboard && !_workTime.Paused && !_workTime.Stopped)
            {
                _workTimeEventService.SetMkEventStart(d, false);
            }
        }


        private void InitSubscriptions()
        {
            _mouseKeyboardMonitor.KeyboardMoveStart += OnKbdStart;
            _mouseKeyboardMonitor.MouseMoveStart += OnMouseStart;
            _keyboardSub = _mouseKeyboardMonitor.KeyboardAction.Subscribe(OnKeyboardAction);
            _mouseSub = _mouseKeyboardMonitor.MouseMoveAction.Subscribe(OnMouseAction);
        }

        public void SetWorkTime(WorkTime workTime)
        {
            _workTime = workTime;
            _workTimeEventService.SetWorkTime(workTime);
        }

        private async void OnMouseAction(MonitorEvent ev)
        {
            if (_canCapureMouseKeyboard && !_workTime.Paused && !_workTime.Stopped)
            {
                Debug.WriteLine("Captured mouse action");
                _workTimeEventService.AddMouseEvent(ev);
            }
            else
            {
                if (_workTime.Stopped)
                {
                    await Stop();
                    Debug.WriteLine("Stopping algorithm on mouse action");
                }

                _workTimeEventService.ResetLastEvents();

                Debug.WriteLine("Ignoring mouse action");
            }
        }

        private async void OnKeyboardAction(MonitorEvent ev)
        {
            if (_canCapureMouseKeyboard && !_workTime.Paused && !_workTime.Stopped)
            {
                Debug.WriteLine("Captured keyboard action");
                _workTimeEventService.AddKeyboardEvent(ev);
            }
            else
            {
                if (_workTime.Stopped)
                {
                    await Stop();
                    Debug.WriteLine("Stopping algorithm on keyboard action");
                }

                _workTimeEventService.ResetLastEvents();

                Debug.WriteLine("Ignoring keyboard action");
            }
        }

        public async void Start()
        {
            if (_sm != null && _sm.CurrentState.Name != States.STOP_STATE)
            {
                await Stop();
            }

            InitSubscriptions();
            InitStateMachine();
            _mouseKeyboardMonitor.Start();
            _workTimeEventService.SetStart();
            _sm.NextAsync(Triggers.Start);
            AlgorithmStarted?.Invoke();
        }

        public async Task Pause()
        {
            await StopStateServices();
            _sm.Next(Triggers.Pause);
            _canCapureMouseKeyboard = true;
        }

        public void Resume()
        {
            _sm.NextAsync(Triggers.Resume);
            _workTimeEventService.ResetLastEvents();
        }

        private Task StopStateServices()
        {
            return Task.Run(async () =>
            {
                do
                {
                    _state2.Cancel();
                    _state3.Cancel();
                    _state5.Cancel();
                    await Task.Delay(200);
                } while (_state2.InProgress || _state3.InProgress || _state5.InProgress);
            });
        }

        public async Task Stop()
        {
            await _stopSem.WaitAsync(TimeSpan.FromSeconds(5));
            _canCapureMouseKeyboard = false;

            if (_sm != null && _sm.CurrentState.Name == States.STOP_STATE)
            {
                return;
            }

            var task = StopStateServices();

#if DEV_MODE
            task = task.ContinueWith(_ => _vis.Dispose());
#endif

            await task;

            _workTimeEventService.TryAddWatchingScreen();
            _workTimeEventService.Flush();

            StopInvoked?.Invoke();

            _sm.Next(Triggers.Stop);

            _stopSem.Release();
        }


        public async Task<bool> StartManualFaceRecog()
        {
            int wait = 0;
            while (_captureService.IsCapturing)
            {
                Log.Logger.Debug("waiting for cap release");
                await Task.Delay(500).ConfigureAwait(true);
                wait++;
                if (wait == 10)
                {
                    Log.Logger.Debug("reached max wait count");
                    return false;
                }
            }

            await StopStateServices();

            _sm.Next(Triggers.ManualTrigger);
            return true;
        }

        public void CancelManualFaceRecog()
        {
            _sm.NextAsync(Triggers.ManualCancel);
        }

        public void SetManualRecogSuccess()
        {
            _sm.NextAsync(Triggers.FaceRecog);
        }
    }
}