using System;
using System.Diagnostics;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Windows.Forms;
using Domain;
using Domain.WorkTimeAggregate.Events;
using Gma.System.MouseKeyHook;

namespace WorkTimeAlghorithm
{
    public interface IMouseKeyboardMonitorService
    {
        IObservable<MonitorEvent> MouseMoveAction { get; }
        IObservable<MonitorEvent> KeyboardAction { get; }
    }

    public class MouseKeyboardMonitorService : IMouseKeyboardMonitorService
    {
        private const int KeyboardDelay = 1000;

        private IKeyboardMouseEvents _hook;
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
                    var ms = (int)(v.Timestamp.DateTime - _mouseStart.Value).TotalMilliseconds - 200;
                    Debug.WriteLine($"Mouse action {ms}");

                    if (ms > 0)
                    {
                        _mouseMoveActionSubject.OnNext(new MonitorEvent()
                        {
                            EventStart = _mouseStart.Value, TotalTimeMs = ms,
                        });
                    }

                    _mouseStart = null;
                });


            _keyboardSubject
                .Throttle(TimeSpan.FromMilliseconds(KeyboardDelay))
                .Timestamp()
                .Subscribe(v =>
                {
                    var ms = (int)(v.Timestamp.DateTime - _keyboardStart.Value).TotalMilliseconds - KeyboardDelay;
                    Debug.WriteLine($"Keyboard action {ms}");

                    if (ms > 0)
                    {
                        _keyboardActionSubject.OnNext(new MonitorEvent()
                        {
                            EventStart = _keyboardStart.Value,
                            TotalTimeMs = ms,
                        });
                    }

                    _keyboardStart = null;
                });

        }

        public void Start()
        {
            _hook = Hook.GlobalEvents();
            _hook.MouseMoveExt += HookOnMouseMoveExt;
            _hook.KeyUp += HookOnKeyUp;
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
