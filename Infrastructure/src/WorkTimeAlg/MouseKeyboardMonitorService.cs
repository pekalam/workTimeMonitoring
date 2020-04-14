using System;
using System.Reactive.Subjects;
using System.Windows.Forms;
using Gma.System.MouseKeyHook;

namespace Infrastructure.WorkTimeAlg
{
    public interface IMouseKeyboardMonitorService
    {
        IObservable<int> MouseAction { get; }
        IObservable<int> KeyboardAction { get; }
    }

    public class MouseKeyboardMonitorService : IMouseKeyboardMonitorService
    {
        private readonly IKeyboardMouseEvents _hook = Hook.GlobalEvents();
        private readonly Subject<int> _mouseSubject = new Subject<int>();
        private readonly Subject<int> _keyboardSubject = new Subject<int>();

        public MouseKeyboardMonitorService()
        {
            _hook.MouseMoveExt += HookOnMouseMoveExt;
            _hook.KeyPress += HookOnKeyPress;
            _hook.MouseClick += HookOnMouseClick;
            _hook.MouseWheel += HookOnMouseWheel;
        }

        public IObservable<int> MouseAction => _mouseSubject;
        public IObservable<int> KeyboardAction => _keyboardSubject;

        private void HookOnKeyPress(object sender, KeyPressEventArgs e)
        {
            _keyboardSubject.OnNext(0);
        }

        private void HookOnMouseMoveExt(object? sender, MouseEventExtArgs e)
        {
            _mouseSubject.OnNext(0);
        }

        private void HookOnMouseWheel(object sender, MouseEventArgs e)
        {
            _mouseSubject.OnNext(0);
        }

        private void HookOnMouseClick(object sender, MouseEventArgs e)
        {
            _mouseSubject.OnNext(0);
        }
    }
}
