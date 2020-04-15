using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Newtonsoft.Json;
using ReflectionMagic;
using Unity.Injection;

namespace Infrastructure.Domain
{
    public class WorkTime
    {
        private readonly List<Event> _pendingEvents = new List<Event>();
        private readonly List<Event> _actionEvents = new List<Event>();

        public WorkTime() { }

        public long AggregateVersion { get; private set; }
        public int AggregateId { get; private set; }
        public DateTime StartDate { get; private set; }
        public DateTime EndDate { get; private set; }
        public User User { get; private set; }

        public IReadOnlyList<Event> PendingEvents => _pendingEvents;
        public IReadOnlyList<Event> ActionEvents => _actionEvents;

        public void MarkPendingEventsAsHandled() => _pendingEvents.Clear();

        private void AddEvent(Event ev)
        {
            ev.AggregateVersion = ++AggregateVersion;
            _pendingEvents.Add(ev);
        }


        public void Start(User user)
        {
            StartDate = DateTime.UtcNow;
            User = user;
            AddEvent(new WorkTimeStarted(AggregateId, DateTime.UtcNow, StartDate, User));
        }

        private void CheckIsStarted()
        {
            if (StartDate == default)
            {
                throw new Exception("WorkTime not started");
            }
        }

        public void AddMouseAction()
        {
            CheckIsStarted();

            var ev = new MouseAction(AggregateId, DateTime.UtcNow, 0);
            _actionEvents.Add(ev);
            AddEvent(ev);
        }

        public void AddKeyboardAction()
        {
            CheckIsStarted();

            var ev = new KeyboardAction(AggregateId, DateTime.UtcNow, 0);
            _actionEvents.Add(ev);
            AddEvent(ev);
        }

        public WorkTimeSnapshotCreated TakeSnapshot()
        {
            CheckIsStarted();

            _actionEvents.Clear();
            var snapshot = new WorkTimeSnapshot()
            {
                StartDate = StartDate,
                User = User,
                EndDate = EndDate,
            };

            var ev = new WorkTimeSnapshotCreated(AggregateId, DateTime.UtcNow, snapshot);
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

            AggregateVersion -= _pendingEvents.Count;
            _pendingEvents.Clear();
            _actionEvents.Clear();

            Apply(workTimeSnapshotEvent);
        }

        private void Apply(WorkTimeSnapshotCreated snapshotCreated)
        {
            var snap = snapshotCreated.Snapshot;
            StartDate = snap.StartDate;
            User = snap.User;
            EndDate = snap.EndDate;
        }

        private void Apply(KeyboardAction keyboardAction)
        {
            _actionEvents.Add(keyboardAction);
        }

        private void Apply(MouseAction mouseAction)
        {
            _actionEvents.Add(mouseAction);
        }

        private void Apply(WorkTimeStarted workTimeStarted)
        {
            StartDate = workTimeStarted.StartDate;
            User = workTimeStarted.User;
        }

        public static WorkTime FromEvents(IReadOnlyList<Event> events)
        {
            var workTime = new WorkTime();
            foreach (var ev in events)
            {
                workTime.AsDynamic().Apply(ev);
            }

            workTime.AggregateVersion = events.Count;

            return workTime;
        }
    }

    public class Event
    {
        public int AggregateId { get; private set; }
        public long AggregateVersion { get; internal set; }
        public DateTime Date { get; }
        public EventName EventName { get; }

        [JsonConstructor]
        public Event(int aggregateId, DateTime date, EventName eventName)
        {
            AggregateId = aggregateId;
            Date = date;
            EventName = eventName;
        }
    }

    public class MouseAction : Event
    {
        public MouseAction(int aggregateId, DateTime date, int totalSeconds) : base(aggregateId, date, EventName.MouseAction)
        {
            TotalSeconds = totalSeconds;
        }

        public int TotalSeconds { get; }
    }

    public class KeyboardAction : Event 
    {
        public KeyboardAction(int aggregateId, DateTime date, int totalSeconds) : base(aggregateId, date, EventName.KeyboardAction)
        {
            TotalSeconds = totalSeconds;
        }

        public int TotalSeconds { get; }
    }


    public enum EventName
    {
        KeyboardAction, MouseAction, WorkTimeStarted, WorkTimeSnapshotCreated
    }

    //
    // public class NoAction : Event { }
    //
    // public class Away : Event { }

    public class WorkTimeStarted : Event
    {
        public WorkTimeStarted(int aggregateId, DateTime date, DateTime startDate, User user) : base(aggregateId, date, EventName.WorkTimeStarted)
        {
            StartDate = startDate;
            User = user;
        }

        public DateTime StartDate { get; }
        public User User { get; }
    }

    public class WorkTimeSnapshotCreated : Event
    {
        public WorkTimeSnapshotCreated(int aggregateId, DateTime date, WorkTimeSnapshot snapshot) : base(aggregateId, date, EventName.WorkTimeSnapshotCreated)
        {
            Snapshot = snapshot;
        }

        public WorkTimeSnapshot Snapshot { get; }
    }

    public class WorkTimeSnapshot
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public User User { get; set; }
    }
}
