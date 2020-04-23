using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
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
        public int WatchingScreenThreshold { get; set; } = 30_000;
    }

    public class WorkTimeEventService
    {
        private readonly MouseKeyboardEventBuilder _mouseEventBuilder;
        private readonly MouseKeyboardEventBuilder _keyboardEventBuilder;
        private readonly IWorkTimeUow _uow;
        private readonly IWorkTimeEsRepository _repository;
        private WorkTime? _workTime;
        private DateTime? _lastMkEvent;
        private DateTime? _recognitionFailureStart;
        private WorkTimeEventServiceSettings _config;

        public WorkTimeEventService(IWorkTimeUow uow, IWorkTimeEsRepository repository, IConfigurationService configurationService)
        {
            var settings = configurationService.Get<WorkTimeEventServiceSettings>("eventGathering");
            _mouseEventBuilder = new MouseKeyboardEventBuilder(settings.MouseEventWindowSz, settings.MouseEventMonitorWindowSz);
            _keyboardEventBuilder = new MouseKeyboardEventBuilder(settings.KeyboardEventWindowSz, settings.KeyboardEventMonitorWindowSz);
            _uow = uow;
            _repository = repository;
            _config = settings;
            MouseEventBufferSz = settings.MouseEventBufferSz;
            KeyboardEventBufferSz = settings.KeyboardEventBufferSz;
        }

        public int MouseEventBufferSz { get; set; }
        public int KeyboardEventBufferSz { get; set; }

        public void SetWorkTime(WorkTime workTime)
        {
            _lastMkEvent = InternalTimeService.GetCurrentDateTime();
            _workTime = workTime;
        }

        public WorkTime? WorkTime => _workTime;


        private void ValidateRecognitionFailureEvent()
        {
            var lastMouse = _workTime.MouseActionEvents.LastOrDefault();
            var lastKey = _workTime.KeyboardActionEvents.LastOrDefault();

            if (lastMouse?.MkEvent.Start.AddMilliseconds(lastMouse.MkEvent.TotalTime) > _recognitionFailureStart.Value)
            {
                throw new ArgumentException("Overlapping recognition failure event");
            }

            if (lastKey?.MkEvent.Start.AddMilliseconds(lastKey.MkEvent.TotalTime) > _recognitionFailureStart.Value)
            {
                throw new ArgumentException("Overlapping recognition failure event");
            }
        }


        public void StartRecognitionFailure() => _recognitionFailureStart = InternalTimeService.GetCurrentDateTime();

        public void StopRecognitionFailure() => _recognitionFailureStart = null;

        public void StartUserWatchingScreen() => _lastMkEvent = InternalTimeService.GetCurrentDateTime();

        public void AddRecognitionFailure(bool faceDetected, bool faceRecognized)
        {
            Debug.Assert(_workTime != null);
            if (_recognitionFailureStart == null)
            {
                throw new Exception("Recognition failure not started");
            }
            ValidateRecognitionFailureEvent();

            Debug.WriteLine("Adding recognition failure");

            _workTime.AddRecognitionFailure(_recognitionFailureStart.Value, faceDetected, faceRecognized);
            _recognitionFailureStart = null;
        }

        private void TryAddUserWatchingScreenEvent(MonitorEvent ev)
        {
            if (_lastMkEvent.HasValue)
            {
                var diff = InternalTimeService.GetCurrentDateTime() - _lastMkEvent.Value;
                if (diff.TotalMilliseconds > _config.WatchingScreenThreshold)
                {
                    Debug.WriteLine("Adding user watching screen");
                    _workTime?.AddUserWatchingScreen(_lastMkEvent.Value);
                }
            }

            _lastMkEvent = ev.EventStart;
        }

        public void AddMouseEvent(MonitorEvent ev)
        {
            Debug.Assert(_workTime != null);
            TryAddUserWatchingScreenEvent(ev);
            if (_mouseEventBuilder.AddEvent(ev, out var created))
            {
                _workTime.AddMouseAction(created);
                SaveIfBufferSz();
            }
        }

        public void AddKeyboardEvent(MonitorEvent ev)
        {
            Debug.Assert(_workTime != null);
            TryAddUserWatchingScreenEvent(ev);
            if (_keyboardEventBuilder.AddEvent(ev, out var created))
            {
                _workTime.AddKeyboardAction(created);
                SaveIfBufferSz();
            }
        }

        private void Flush()
        {
            Debug.Assert(_workTime != null);
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
            Debug.Assert(_workTime != null);
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
            Debug.Assert(_workTime != null);
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
            Debug.Assert(_workTime != null);
            Debug.WriteLine("Starting temp changes");
            if (_uow.HasRegistered)
            {
                _uow.Unregister(_workTime);
            }
            _uow.RegisterNew(_workTime);
        }

        public void DiscardTempChanges()
        {
            Debug.Assert(_workTime != null);
            Debug.WriteLine("Discarding temp changes");
            _lastMkEvent = null;
            _keyboardEventBuilder.Reset();
            _mouseEventBuilder.Reset();
            _uow.Rollback();
            _uow.Unregister(_workTime);
        }

        public void CommitTempChanges()
        {
            Debug.Assert(_workTime != null);
            Debug.WriteLine("Commiting temp changes");
            _uow.Commit();
            _uow.Unregister(_workTime);
        }
    }
}
