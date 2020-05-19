using Domain.Repositories;
using Domain.WorkTimeAggregate;
using Domain.WorkTimeAggregate.Events;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

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

    internal class MKEventBuilders
    {
        private WorkTimeEventServiceSettings _config;

        public MKEventBuilders(WorkTimeEventServiceSettings config, string executable)
        {
            _config = config;
            MouseEventBuilder = new MouseKeyboardEventBuilder(_config.MouseEventWindowSz, _config.MouseEventMonitorWindowSz, executable);
            KeyboardEventBuilder = new MouseKeyboardEventBuilder(_config.KeyboardEventWindowSz, _config.KeyboardEventMonitorWindowSz, executable);
        }

        public readonly MouseKeyboardEventBuilder MouseEventBuilder;
        public readonly MouseKeyboardEventBuilder KeyboardEventBuilder;
    }



    public class WorkTimeEventService
    {
        private const string UnknownExecutableName = "Unknown";
        private readonly Dictionary<string, MKEventBuilders> _eventBuilders = new Dictionary<string, MKEventBuilders>();
        private readonly MouseKeyboardEventBuilder _allMouseBuilder;
        private readonly MouseKeyboardEventBuilder _allKeyboardBuilder;
        private readonly IWorkTimeUow _uow;
        private readonly IWorkTimeEsRepository _repository;
        private readonly WorkTimeEventServiceSettings _config;
        private DateTime? _lastMkEventStart;
        private DateTime? _lastMkEventEnd;
        private DateTime? _recognitionFailureStart;
        private string _lastActiveWinExecutable;
        private bool? _lastMouseOrKey;

        public WorkTimeEventService(IWorkTimeUow uow, IWorkTimeEsRepository repository, IConfigurationService configurationService)
        {
            var settings = configurationService.Get<WorkTimeEventServiceSettings>("eventGathering");
            _uow = uow;
            _repository = repository;
            _config = settings;
            MouseEventBufferSz = settings.MouseEventBufferSz;
            KeyboardEventBufferSz = settings.KeyboardEventBufferSz;
            _allKeyboardBuilder = new MouseKeyboardEventBuilder(_config.KeyboardEventWindowSz, _config.KeyboardEventMonitorWindowSz, string.Empty);
            _allMouseBuilder = new MouseKeyboardEventBuilder(_config.MouseEventWindowSz, _config.MouseEventMonitorWindowSz, string.Empty);
        }

        public int MouseEventBufferSz { get; set; }
        public int KeyboardEventBufferSz { get; set; }

        public WorkTime? WorkTime { get; private set; }

        private void ValidateRecognitionFailureEvent()
        {
            Debug.Assert(WorkTime != null);
            var lastMouse = WorkTime.MouseActionEvents.LastOrDefault();
            var lastKey = WorkTime.KeyboardActionEvents.LastOrDefault();

            if (lastMouse?.MkEvent.Start.AddMilliseconds(lastMouse.MkEvent.TotalTime) > _recognitionFailureStart.Value)
            {
                throw new ArgumentException("Overlapping recognition failure event");
            }

            if (lastKey?.MkEvent.Start.AddMilliseconds(lastKey.MkEvent.TotalTime) > _recognitionFailureStart.Value)
            {
                throw new ArgumentException("Overlapping recognition failure event");
            }
        }

        public void SetWorkTime(WorkTime workTime)
        {
            WorkTime = workTime;
        }

        public void StartRecognitionFailure() => _recognitionFailureStart = InternalTimeService.GetCurrentDateTime();

        public void StopRecognitionFailure() => _recognitionFailureStart = null;

        public void TryStartWatchingScreen()
        {
            if (_lastMkEventEnd == null)
            {
                _lastMkEventEnd = InternalTimeService.GetCurrentDateTime();
            }
        }
    

        public void ResetLastEvents()
        {
            _lastActiveWinExecutable = UnknownExecutableName;
            _lastMkEventStart = null;
            _lastMouseOrKey = null;
            _lastMkEventEnd = null;
        }

        public void AddRecognitionFailure(bool faceDetected, bool faceRecognized)
        {
            Debug.Assert(WorkTime != null);
            if (_recognitionFailureStart == null)
            {
                throw new Exception("Recognition failure not started");
            }
            ValidateRecognitionFailureEvent();

            Debug.WriteLine("Adding recognition failure");
            WorkTime.AddRecognitionFailure(_recognitionFailureStart.Value, faceDetected, faceRecognized);
            _recognitionFailureStart = null;
        }

        private MKEventBuilders GetEventBuilder(string? executable)
        {
            if (executable == null)
            {
                Debug.WriteLine("Null executable");
                executable = UnknownExecutableName;
            }

            if (_eventBuilders.TryGetValue(executable, out var builders))
            {
                return builders;
            }
            else
            {
                builders =  new MKEventBuilders(_config, executable);
                _eventBuilders[executable] = builders;
                return builders;
            }
        }

        public void SetMkEventStart(DateTime date, bool isMouseEvent)
        {
            if (!_lastMkEventStart.HasValue || (_lastMouseOrKey.HasValue && _lastMouseOrKey == isMouseEvent))
            {
                _lastMkEventStart = date;
                _lastMouseOrKey = isMouseEvent;

                if (_lastMkEventEnd.HasValue)
                {
                    var diff = date - _lastMkEventEnd.Value;
                    if (diff.TotalMilliseconds > _config.WatchingScreenThreshold)
                    {
                        Debug.WriteLine("Adding user watching screen diff: " + diff.TotalMilliseconds);
                        WorkTime?.AddUserWatchingScreen(_lastMkEventEnd.Value, (long)diff.TotalMilliseconds, _lastActiveWinExecutable);
                    }

                    _lastMkEventEnd = null;
                }
                
            }
        }

        public void TryAddWatchingScreen()
        {
            if (_lastMkEventEnd.HasValue)
            {
                var diff = InternalTimeService.GetCurrentDateTime() - _lastMkEventEnd.Value;
                if (diff.TotalMilliseconds > _config.WatchingScreenThreshold)
                {
                    Debug.WriteLine("Adding user watching screen diff: " + diff.TotalMilliseconds);
                    WorkTime?.AddUserWatchingScreen(_lastMkEventEnd.Value, (long)diff.TotalMilliseconds, _lastActiveWinExecutable);
                }

                _lastMkEventEnd = null;
            }
        }

        public void SetStart()
        {
            _lastMkEventEnd = InternalTimeService.GetCurrentDateTime();
            _lastActiveWinExecutable = UnknownExecutableName;
        }

        private void SetEventEnd(MonitorEvent ev)
        {
            _lastMkEventEnd = ev.EventStart.AddMilliseconds(ev.TotalTimeMs);
            _lastActiveWinExecutable = ev.Executable ?? UnknownExecutableName;

            if (_lastMkEventStart.HasValue && ev.EventStart == _lastMkEventStart.Value)
            {
                _lastMkEventStart = null;
                _lastMouseOrKey = null;
            }
        }

        public void AddMouseEvent(MonitorEvent ev)
        {
            Debug.Assert(WorkTime != null);
            SetEventEnd(ev);
            if (GetEventBuilder(ev.Executable).MouseEventBuilder.AddEvent(ev, out var created))
            {
                WorkTime.AddMouseAction(created);
                SaveIfBufferSz();
                _allMouseBuilder.AddEvent(ev, out created);
            }
            else if (_allMouseBuilder.AddEvent(ev, out created))
            {
                SaveIfBufferSz();   
            }
        }

        public void AddKeyboardEvent(MonitorEvent ev)
        {
            Debug.Assert(WorkTime != null);
            SetEventEnd(ev);
            if (GetEventBuilder(ev.Executable).KeyboardEventBuilder.AddEvent(ev, out var created))
            {
                WorkTime.AddKeyboardAction(created);
                SaveIfBufferSz();
                _allKeyboardBuilder.AddEvent(ev, out created);
            }
            else if (_allKeyboardBuilder.AddEvent(ev, out created))
            {
                SaveIfBufferSz();
            }
        }

        private void Flush()
        {
            Debug.Assert(WorkTime != null);
            foreach (var builders in _eventBuilders.Values)
            {
                var mouse = builders.MouseEventBuilder.Flush();
                var keyboard = builders.KeyboardEventBuilder.Flush();

                if (keyboard != null)
                {
                    WorkTime.AddKeyboardAction(keyboard);
                }

                if (mouse != null)
                {
                    WorkTime.AddMouseAction(mouse);
                }
            }
        }

        private void SavePendingEvents()
        {
            Debug.Assert(WorkTime != null);
            if (_uow.HasRegistered)
            {
                Debug.WriteLine("Saving temporary mk events");
                _uow.Save();
                WorkTime.ClearEvents();
            }
            else
            {
                Debug.WriteLine("Saving mk events");
                _repository.Save(WorkTime);
                WorkTime.MarkPendingEventsAsHandled();
                WorkTime.ClearEvents();
            }
        }
        
        private void SaveIfBufferSz()
        {
            Debug.Assert(WorkTime != null);
            Flush();
            if (WorkTime.KeyboardActionEvents.Count > KeyboardEventBufferSz)
            {
                Debug.WriteLine($"Keyboard events count ({WorkTime.KeyboardActionEvents.Count}) exceeded buffer sz {KeyboardEventBufferSz}");
                SavePendingEvents();
            }
            else if (WorkTime.MouseActionEvents.Count > MouseEventBufferSz)
            {
                Debug.WriteLine($"Mouse events count ({WorkTime.MouseActionEvents.Count}) exceeded buffer sz {MouseEventBufferSz}");
                SavePendingEvents();
            }
            //TODO watching screen window
            else if (WorkTime.UserWatchingScreen.Count > 10)
            {
                Debug.WriteLine($"UserWatchingScreen events count ({WorkTime.UserWatchingScreen.Count}) exceeded buffer sz {10}");
                SavePendingEvents();
            }

        }

        public void StartTempChanges()
        {
            Debug.Assert(WorkTime != null);
            Debug.WriteLine("Starting temp changes");
            if (_uow.HasRegistered)
            {
                _uow.Unregister(WorkTime);
            }
            _uow.RegisterNew(WorkTime);
        }

        public void DiscardTempChanges()
        {
            Debug.Assert(WorkTime != null);
            Debug.WriteLine("Discarding temp changes");
            ResetLastEvents();
            foreach (var builders in _eventBuilders.Values)
            {
                builders.MouseEventBuilder.Reset();
                builders.KeyboardEventBuilder.Reset();
            }
            _allKeyboardBuilder.Reset();
            _allMouseBuilder.Reset();
            _uow.Rollback();
            _uow.Unregister(WorkTime);
        }

        public void CommitTempChanges()
        {
            Debug.Assert(WorkTime != null);
            Debug.WriteLine("Commiting temp changes");
            _uow.Commit();
            _uow.Unregister(WorkTime);
        }
    }
}
