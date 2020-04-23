using System;
using System.Diagnostics;
using System.Drawing;
using Domain.Services;
using Domain.WorkTimeAggregate;
using Domain.WorkTimeAggregate.Events;

namespace WorkTimeAlghorithm.StateMachine
{
    public partial class WMonitorAlghorithm
    {
        private MouseKeyboardMonitorService _mouseKeyboardMonitor = new MouseKeyboardMonitorService();
        private WorkTimeEventService _workTimeEventService;
        private State2Service _state2;
        private State3Service _state3;
        private State5Service _state5 = new State5Service();
        private WorkTime _workTime;

        public WMonitorAlghorithm(AlghorithmFaceRecognition faceRecognition, WorkTimeEventService workTimeEventService, IConfigurationService configurationService)
        {
            _mouseKeyboardMonitor.KeyboardAction.Subscribe(OnKeyboardAction);
            _mouseKeyboardMonitor.MouseMoveAction.Subscribe(OnMouseAction);
            _workTimeEventService = workTimeEventService;
            _state2 = new State2Service(faceRecognition, workTimeEventService, configurationService);
            _state3 = new State3Service(faceRecognition, workTimeEventService, configurationService);
            InitStateMachine();
        }

        public void SetWorkTime(WorkTime workTime)
        {
            _workTime = workTime;
            _workTimeEventService.SetWorkTime(workTime);
        }

        private void OnMouseAction(MonitorEvent ev)
        {
            if (_state.CanCapureMouseKeyboard)
            {
                Debug.WriteLine("Captured mouse action");
                _workTimeEventService.AddMouseEvent(ev);
                _sm.Next(Triggers.MouseMv);
            }
            else
            {
                Debug.WriteLine("Ignoring mouse action");
            }
        }

        private void OnKeyboardAction(MonitorEvent ev)
        {
            if (_state.CanCapureMouseKeyboard)
            {
                Debug.WriteLine("Captured keyboard action");
                _workTimeEventService.AddKeyboardEvent(ev);
                _sm.Next(Triggers.KeyboardMv);
            }
            else
            {
                Debug.WriteLine("Ignoring keyboard action");
            }
        }

        public void Start()
        {
            _mouseKeyboardMonitor.Start();
            _sm.Next(Triggers.Start);
        }
    }
}