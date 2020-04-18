using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using Domain.Repositories;
using Domain.WorkTimeAggregate;
using Domain.WorkTimeAggregate.Events;

namespace Domain.Services
{
    public class WorkTimeEventServiceSettings
    {
        public int MouseEventWindowSz { get; set; } = 60_000;
        public int MouseEventMonitorWindowSz { get; set; } = 1000;
        public int KeyboardEventWindowSz { get; set; } = 60_000;
        public int KeyboardEventMonitorWindowSz { get; set; } = 1000;
        public int MouseEventBufferSz { get; set; } = 10;
        public int KeyboardEventBufferSz { get; set; } = 10;
    }

    public class WorkTimeEventService
    {
        private readonly MouseKeyboardEventBuilder _mouseEventBuilder;
        private readonly MouseKeyboardEventBuilder _keyboardEventBuilder;
        private readonly IWorkTimeUow _uow;
        private readonly IWorkTimeEsRepository _repository;
        private WorkTime _workTime;
            
        public WorkTimeEventService(IWorkTimeUow uow, IWorkTimeEsRepository repository, IConfigurationService configurationService)
        {
            var settings = configurationService.Get<WorkTimeEventServiceSettings>("eventGathering");
            _mouseEventBuilder = new MouseKeyboardEventBuilder(settings.MouseEventWindowSz, settings.MouseEventMonitorWindowSz);
            _keyboardEventBuilder = new MouseKeyboardEventBuilder(settings.KeyboardEventWindowSz, settings.KeyboardEventMonitorWindowSz);
            _uow = uow;
            _repository = repository;
            MouseEventBufferSz = settings.MouseEventBufferSz;
            KeyboardEventBufferSz = settings.KeyboardEventBufferSz;
        }

        public int MouseEventBufferSz { get; set; }
        public int KeyboardEventBufferSz { get; set; }

        public void SetWorkTime(WorkTime workTime) => _workTime = workTime;
        public WorkTime WorkTime => _workTime;

        public void AddMouseEvent(MonitorEvent ev)
        {
            if (_mouseEventBuilder.AddEvent(ev, out var created))
            {
                _workTime.AddMouseAction(created);
                SaveIfBufferSz();
            }
        }

        public void AddKeyboardEvent(MonitorEvent ev)
        {
            if (_keyboardEventBuilder.AddEvent(ev, out var created))
            {
                _workTime.AddKeyboardAction(created);
                SaveIfBufferSz();
            }
        }

        private void Flush()
        {
            var keyboard = _keyboardEventBuilder.Flush();
            var mouse = _mouseEventBuilder.Flush();

            if (keyboard != null)
            {
                _workTime.AddKeyboardAction(keyboard);
            }

            if (mouse != null)
            {
                _workTime.AddMouseAction(mouse);
            }
        }

        private void SavePendingEvents()
        {
            if (_uow.HasRegistered)
            {
                Debug.WriteLine("Saving temporary mk events");
                _uow.Save();
                _workTime.ClearEvents();
            }
            else
            {
                Debug.WriteLine("Saving mk events");
                _repository.Save(_workTime);
                _workTime.MarkPendingEventsAsHandled();
                _workTime.ClearEvents();
            }
        }
        
        private void SaveIfBufferSz()
        {
            if (_workTime.KeyboardActionEvents.Count > KeyboardEventBufferSz)
            {
                Debug.WriteLine($"Keyboard events count ({_workTime.KeyboardActionEvents.Count}) exceeded buffer sz {KeyboardEventBufferSz}");
                SavePendingEvents();
            }
            else if (_workTime.MouseActionEvents.Count > MouseEventBufferSz)
            {
                Debug.WriteLine($"Mouse events count ({_workTime.MouseActionEvents.Count}) exceeded buffer sz {MouseEventBufferSz}");
                SavePendingEvents();
            }

        }

        public void StartTempChanges()
        {
            Debug.WriteLine("Starting temp changes");
            if (_uow.HasRegistered)
            {
                _uow.Unregister(_workTime);
            }
            _uow.RegisterNew(_workTime);
        }

        public void DiscardTempChanges()
        {
            Debug.WriteLine("Discarding temp changes");
            _uow.Rollback();
            _uow.Unregister(_workTime);
        }

        public void CommitTempChanges()
        {
            Debug.WriteLine("Commiting temp changes");
            _uow.Commit();
            _uow.Unregister(_workTime);
        }
    }
}
