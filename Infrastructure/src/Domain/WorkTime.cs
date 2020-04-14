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
        private List<Event> _pendingEvents = new List<Event>();
        private List<Event> _actionEvents = new List<Event>();

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
            AddEvent(new WorkTimeStarted(AggregateId, StartDate, User));
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

            var ev = new MouseAction(AggregateId);
            _actionEvents.Add(ev);
            AddEvent(ev);
        }

        public void AddKeyboardAction()
        {
            CheckIsStarted();

            var ev = new KeyboardAction(AggregateId);
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
            };

            var ev = new WorkTimeSnapshotCreated(AggregateId, snapshot);
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
        public DateTime OccurrenceDate { get; }

        public Event(int aggregateId)
        {
            AggregateId = aggregateId;
            OccurrenceDate = DateTime.UtcNow;
        }

        [JsonConstructor]
        public Event(int aggregateId, DateTime occurrenceDate)
        {
            AggregateId = aggregateId;
            OccurrenceDate = occurrenceDate;
        }
    }

    public class MouseAction : Event
    {
        public MouseAction(int aggregateId) : base(aggregateId)
        {
        }
    }
    
    public class KeyboardAction : Event {
        public KeyboardAction(int aggregateId) : base(aggregateId)
        {
        }
    }
    //
    // public class NoAction : Event { }
    //
    // public class Away : Event { }

    public class WorkTimeStarted : Event
    {
        public WorkTimeStarted(int aggregateId, DateTime startDate, User user) : base(aggregateId)
        {
            StartDate = startDate;
            User = user;
        }

        public DateTime StartDate { get; }
        public User User { get; }
    }

    public class WorkTimeSnapshotCreated : Event
    {
        public WorkTimeSnapshotCreated(int aggregateId, WorkTimeSnapshot snapshot) : base(aggregateId)
        {
            Snapshot = snapshot;
        }

        public WorkTimeSnapshot Snapshot { get; }
    }

    public class WorkTimeSnapshot
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; private set; }
        public User User { get; private set; }
    }
}
