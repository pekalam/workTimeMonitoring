using System;
using System.Collections.Generic;
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
        private readonly List<Event> _pendingEvents = new List<Event>();
        private readonly List<MouseAction> _mouseActionEvents = new List<MouseAction>();
        private readonly List<KeyboardAction> _keyboardActionEvents = new List<KeyboardAction>();
        private readonly List<FaceRecognitionFailure> _recognitionFailureEvents = new List<FaceRecognitionFailure>();


        internal WorkTime(long aggregateId, User.User user, DateTime? startDate, DateTime endDate)
        {
            AggregateId = aggregateId;
            Create(user, startDate, endDate);
        }

        private WorkTime() { }


        public long AggregateVersion { get; private set; }
        public long AggregateId { get; private set; } = -1;
        public DateTime? StartDate { get; private set; }
        public DateTime EndDate { get; private set; }
        public DateTime DateCreated { get; private set; }
        public User.User User { get; private set; }
        public bool AutoStart { get; private set; }
        public bool Started => StartDate.HasValue;
        public bool FromSnapshot { get; private set; }

        public IReadOnlyList<Event> PendingEvents => _pendingEvents;
        public IReadOnlyList<MouseAction> MouseActionEvents => _mouseActionEvents;
        public IReadOnlyList<KeyboardAction> KeyboardActionEvents => _keyboardActionEvents;
        public IReadOnlyList<FaceRecognitionFailure> FaceRecognitionFailures => _recognitionFailureEvents;

        private void AddEvent(Event ev)
        {
            ev.AggregateVersion = ++AggregateVersion;
            _pendingEvents.Add(ev);
        }


        private void Create(User.User user, DateTime? startDate, DateTime endDate)
        {
            StartDate = startDate;
            AutoStart = startDate.HasValue;
            User = user;
            EndDate = endDate;
            var now = InternalTimeService.GetCurrentDateTime();
            DateCreated = now;
            AddEvent(new WorkTimeCreated(AggregateId, now, StartDate, EndDate,DateCreated, User, AutoStart));
        }

        private void CheckIsStarted()
        {
            if (!StartDate.HasValue)
            {
                throw new Exception("WorkTime not started");
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

        internal void AddMouseAction(MouseKeyboardEvent mkEvent)
        {
            CheckIsStarted();

            var ev = new MouseAction(AggregateId, InternalTimeService.GetCurrentDateTime(),  mkEvent);
            _mouseActionEvents.Add(ev);
            AddEvent(ev);
        }

        internal void AddKeyboardAction(MouseKeyboardEvent mkEvent)
        {
            CheckIsStarted();

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

        internal void AddRecognitionFailure(bool faceDetected, bool faceRecognized)
        {
            CheckIsStarted();

            var ev = new FaceRecognitionFailure(AggregateId, InternalTimeService.GetCurrentDateTime(), faceRecognized, faceDetected);
            _recognitionFailureEvents.Add(ev);
            AddEvent(ev);
        }

        public WorkTimeSnapshotCreated TakeSnapshot()
        {
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
            CheckIsStarted();
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
