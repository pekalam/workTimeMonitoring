using System;
using System.Diagnostics;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;
using Domain;
using Domain.WorkTimeAggregate.Events;
using Gma.System.MouseKeyHook;
using Serilog;

namespace WorkTimeAlghorithm
{
    public interface IMouseKeyboardMonitorService
    {
        IObservable<MonitorEvent> MouseMoveAction { get; }
        IObservable<MonitorEvent> KeyboardAction { get; }
        void Start();
        void Stop();
    }


    public class MouseKeyboardMonitorService : IMouseKeyboardMonitorService
    {
        [DllImport("user32.dll")]
        static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        static extern int GetWindowThreadProcessId(IntPtr handle, out uint processId);

        

        private const int KeyboardDelay = 1000;
        private IntPtr _lastWin = IntPtr.Zero;
        private string? _lastWinExePath;

        private IKeyboardMouseEvents? _hook;
        private readonly Subject<int> _mouseMoveSubject = new Subject<int>();
        private readonly Subject<MonitorEvent> _mouseMoveActionSubject = new Subject<MonitorEvent>();
        private readonly Subject<int> _keyboardSubject = new Subject<int>();
        private readonly Subject<MonitorEvent> _keyboardActionSubject = new Subject<MonitorEvent>();

        private DateTime? _mouseStart;
        private DateTime? _keyboardStart;

        public MouseKeyboardMonitorService()
        {


            _mouseMoveSubject
                .Throttle(TimeSpan.FromMilliseconds(200))
                .Timestamp()
                .Subscribe(v =>
                {
                    if (_mouseStart == null)
                    {
                        return;
                    }
                    var ms = (int)(v.Timestamp.DateTime - _mouseStart.Value).TotalMilliseconds - 200;
                    Debug.WriteLine($"Mouse action {ms} {GetActiveWindowExecutable()}");

                    if (ms > 0)
                    {
                        _mouseMoveActionSubject.OnNext(new MonitorEvent()
                        {
                            EventStart = _mouseStart.Value, TotalTimeMs = ms,
                            Executable = GetActiveWindowExecutable(),
                        });
                    }

                    _mouseStart = null;
                });


            _keyboardSubject
                .Throttle(TimeSpan.FromMilliseconds(KeyboardDelay))
                .Timestamp()
                .Subscribe(v =>
                {
                    if (_keyboardStart == null)
                    {
                        return;
                    }
                    var ms = (int)(v.Timestamp.DateTime - _keyboardStart.Value).TotalMilliseconds - KeyboardDelay;
                    Debug.WriteLine($"Keyboard action {ms} {GetActiveWindowExecutable()}");

                    if (ms > 0)
                    {
                        _keyboardActionSubject.OnNext(new MonitorEvent()
                        {
                            EventStart = _keyboardStart.Value,
                            TotalTimeMs = ms,
                            Executable = GetActiveWindowExecutable(),
                        });
                    }

                    _keyboardStart = null;
                });

        }

        private string? GetActiveWindowExecutable([CallerMemberName] string method = "")
        {
            var hWin = GetForegroundWindow();
            if (hWin != _lastWin)
            {
                GetWindowThreadProcessId(hWin, out var pid);
                _lastWin = hWin;
                try
                {
                    var path = Process.GetProcessById((int)pid).MainModule?.FileName;
                    _lastWinExePath = path?.Split('\\', StringSplitOptions.RemoveEmptyEntries).LastOrDefault();
                    return _lastWinExePath;
                }
                catch (Exception e)
                {
                    Log.Logger.Debug(e, $"{method} GetProccessById exception. pid: {pid} hWin: {hWin}");
                    _lastWin = IntPtr.Zero;
                }
            }
            else
            {
                return _lastWinExePath;
            }

            return null;
        }

        public void Start()
        {
            _hook = Hook.GlobalEvents();
            _hook.MouseMoveExt += HookOnMouseMoveExt;
            _hook.KeyUp += HookOnKeyUp;
        }

        public void Stop()
        {
            _hook.MouseMoveExt -= HookOnMouseMoveExt;
            _hook.KeyUp -= HookOnKeyUp;
            _hook.Dispose();
        }

        private void HookOnKeyUp(object sender, KeyEventArgs e)
        {
            if (!_keyboardStart.HasValue)
            {
                _keyboardStart = DateTime.UtcNow;
            }
            _keyboardSubject.OnNext(0);
        }

        public IObservable<MonitorEvent> MouseMoveAction => _mouseMoveActionSubject;
        public IObservable<MonitorEvent> KeyboardAction => _keyboardActionSubject;

        private void HookOnMouseMoveExt(object? sender, MouseEventExtArgs e)
        {
            if (!_mouseStart.HasValue)
            {
                _mouseStart = DateTime.UtcNow;
            }
            _mouseMoveSubject.OnNext(0);
        }
    }
}
