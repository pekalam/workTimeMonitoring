using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using Domain.WorkTimeAggregate.Events;

[assembly: InternalsVisibleTo("Domain.UnitTests")]
[assembly: InternalsVisibleTo("Infrastructure.Tests")]

namespace Domain.WorkTimeAggregate
{
    internal static class InternalTimeService
    {
        public static Func<DateTime> GetCurrentDateTime = () => DateTime.UtcNow;
    }

    public class WorkTimeSnapshot
    {
        public DateTime? StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public DateTime DateCreated { get; set; }
        public User.User User { get; set; }
        public bool AutoStart { get; set; }
    }

    public partial class WorkTime
    {
    }

    public partial class WorkTime
    {
        private readonly List<Event> _pendingEvents = new List<Event>();
        private readonly List<MouseAction> _mouseActionEvents = new List<MouseAction>();
        private readonly List<KeyboardAction> _keyboardActionEvents = new List<KeyboardAction>();
        private readonly List<FaceRecognitionFailure> _recognitionFailureEvents = new List<FaceRecognitionFailure>();
        private readonly List<UserWatchingScreen> _userWatchingScreenEvents = new List<UserWatchingScreen>();

        internal WorkTime(long aggregateId, User.User user, DateTime? startDate, DateTime endDate)
        {
            AggregateId = aggregateId;
            Create(user, startDate, endDate);
        }

        private WorkTime()
        {
        }

        private WorkTimeInterrupted? _lastInterruptedEvent;

        public long AggregateVersion { get; private set; }
        public long AggregateId { get; private set; } = -1;
        public DateTime? StartDate { get; private set; }
        public DateTime EndDate { get; private set; }
        public DateTime DateCreated { get; private set; }
        public User.User User { get; private set; }
        public bool AutoStart { get; private set; }
        public bool Started => StartDate.HasValue;
        public bool Stopped => EndDate < InternalTimeService.GetCurrentDateTime() || StoppedByUser;
        public bool StoppedByUser { get; private set; }
        public bool Paused { get; private set; }
        public bool FromSnapshot { get; private set; }
        public bool Interrupted => _lastInterruptedEvent != null;

        public IReadOnlyList<Event> PendingEvents => _pendingEvents;
        public IReadOnlyList<MouseAction> MouseActionEvents => _mouseActionEvents;
        public IReadOnlyList<KeyboardAction> KeyboardActionEvents => _keyboardActionEvents;
        public IReadOnlyList<FaceRecognitionFailure> FaceRecognitionFailures => _recognitionFailureEvents;
        public IReadOnlyList<UserWatchingScreen> UserWatchingScreen => _userWatchingScreenEvents;


        private void Create(User.User user, DateTime? startDate, DateTime endDate)
        {
            StartDate = startDate;
            AutoStart = startDate.HasValue;
            User = user;
            EndDate = endDate;
            var now = InternalTimeService.GetCurrentDateTime();
            DateCreated = now;
            AddEvent(new WorkTimeCreated(AggregateId, now, StartDate, EndDate, DateCreated, User, AutoStart));
        }

        private void CheckIsStarted()
        {
            if (!Started)
            {
                throw new Exception("WorkTime not started");
            }
        }

        private void CheckNotStopped()
        {
            if (Stopped)
            {
                throw new Exception("WorkTime stopped");
            }

            if (Paused)
            {
                throw new Exception("WorkTime paused");
            }
        }

        private void CheckNotInterrupted()
        {
            if (_lastInterruptedEvent != null)
            {
                throw new Exception("WorkTime not restored");
            }
        }

        private void StartWorkTime()
        {
            var now = InternalTimeService.GetCurrentDateTime();
            StartDate = now;
            AddEvent(new WorkTimeStarted(AggregateId, now, StartDate.Value));
        }

        public void StartManually()
        {
            if (AutoStart)
            {
                throw new Exception("Work time should be automatically started");
            }

            StartWorkTime();
        }

        public void Start()
        {
            if (!AutoStart)
            {
                throw new Exception("Work time not set to autoStart");
            }

            StartWorkTime();
        }

        public void Stop()
        {
            CheckIsStarted();
            CheckNotStopped();
            CheckNotInterrupted();

            StoppedByUser = true;
        }

        public void Pause()
        {
            CheckIsStarted();
            CheckNotStopped();
            CheckNotInterrupted();

            Paused = true;
        }

        public void Resume()
        {
            CheckIsStarted();
            CheckNotInterrupted();

            Paused = false;
        }

        internal void AddMouseAction(MouseKeyboardEvent mkEvent)
        {
            CheckIsStarted();
            CheckNotStopped();
            CheckNotInterrupted();

            var ev = new MouseAction(AggregateId, InternalTimeService.GetCurrentDateTime(), mkEvent);
            _mouseActionEvents.Add(ev);
            AddEvent(ev);
        }

        internal void AddKeyboardAction(MouseKeyboardEvent mkEvent)
        {
            CheckIsStarted();
            CheckNotStopped();
            CheckNotInterrupted();

            var ev = new KeyboardAction(AggregateId, InternalTimeService.GetCurrentDateTime(), mkEvent);
            _keyboardActionEvents.Add(ev);
            AddEvent(ev);
        }

        internal void ClearEvents()
        {
            _keyboardActionEvents.Clear();
            _mouseActionEvents.Clear();
            _recognitionFailureEvents.Clear();
        }

        internal void AddRecognitionFailure(DateTime startDate, bool faceDetected, bool faceRecognized)
        {
            CheckIsStarted();
            CheckNotStopped();
            CheckNotInterrupted();

            var end = InternalTimeService.GetCurrentDateTime();
            if (end <= startDate)
            {
                throw new Exception();
            }
            var ev = new FaceRecognitionFailure(AggregateId, startDate,
                faceRecognized, faceDetected, end, (long)(end - startDate).TotalMilliseconds);
            _recognitionFailureEvents.Add(ev);
            AddEvent(ev);
        }

        internal void AddUserWatchingScreen(DateTime startDate, string executable)
        {
            CheckIsStarted();
            CheckNotStopped();
            CheckNotInterrupted();

            var end = InternalTimeService.GetCurrentDateTime();
            if (end <= startDate)
            {
                throw new Exception();
            }
            var ev = new UserWatchingScreen(AggregateId, startDate, end, (long)(end - startDate).TotalMilliseconds, executable);
            _userWatchingScreenEvents.Add(ev);
            AddEvent(ev);
        }

        internal void SetInterrupted()
        {
            CheckIsStarted();
            CheckNotStopped();

            var ev = new WorkTimeInterrupted(AggregateId, InternalTimeService.GetCurrentDateTime());
            _lastInterruptedEvent = ev;
            AddEvent(ev);
        }

        internal void SetRestored()
        {
            CheckIsStarted();
            CheckNotStopped();

            WorkTimeRestored ev;
            var now = InternalTimeService.GetCurrentDateTime();
            if (_lastInterruptedEvent != null)
            {
                ev = new WorkTimeRestored(AggregateId, now, (long)(now - _lastInterruptedEvent.Date).TotalMilliseconds);
            }
            else
            {
                ev = new WorkTimeRestored(AggregateId, now, 0);
            }
            _lastInterruptedEvent = null;
            AddEvent(ev);
        }

        public WorkTimeSnapshotCreated TakeSnapshot()
        {
            CheckNotInterrupted();

            ClearEvents();
            var snapshot = new WorkTimeSnapshot()
            {
                StartDate = StartDate,
                User = User,
                EndDate = EndDate,
                AutoStart = AutoStart,
                DateCreated = DateCreated,
            };

            var ev = new WorkTimeSnapshotCreated(AggregateId, InternalTimeService.GetCurrentDateTime(), snapshot);
            AddEvent(ev);
            return ev;
        }

        public void RollbackToSnapshot(WorkTimeSnapshotCreated workTimeSnapshotEvent)
        {
            if (workTimeSnapshotEvent.AggregateId != AggregateId)
            {
                throw new Exception("Invalid snapshot aggregateId");
            }

            AggregateVersion = workTimeSnapshotEvent.AggregateVersion;
            FromSnapshot = true;
            _pendingEvents.Clear();
            ClearEvents();

            Apply(workTimeSnapshotEvent);
        }
    }
}