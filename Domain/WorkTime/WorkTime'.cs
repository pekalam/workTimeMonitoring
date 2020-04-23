using System;
using System.Collections.Generic;
using System.Linq;
using Domain.WorkTimeAggregate.Events;
using ReflectionMagic;

namespace Domain.WorkTimeAggregate
{
    public partial class WorkTime
    {
        public void MarkPendingEventsAsHandled() => _pendingEvents.Clear();

        private void Apply(WorkTimeSnapshotCreated snapshotCreated)
        {
            var snap = snapshotCreated.Snapshot;
            StartDate = snap.StartDate;
            User = snap.User;
            EndDate = snap.EndDate;
            AutoStart = snap.AutoStart;
            DateCreated = snap.DateCreated;
        }

        private void Apply(KeyboardAction keyboardAction)
        {
            _keyboardActionEvents.AddLast(keyboardAction);
        }

        private void Apply(MouseAction mouseAction)
        {
            _mouseActionEvents.AddLast(mouseAction);
        }

        private void Apply(WorkTimeStarted workTimeStarted)
        {
            StartDate = workTimeStarted.StartDate;
        }

        private void Apply(FaceRecognitionFailure faceRecognitionFailure)
        {
            _recognitionFailureEvents.Add(faceRecognitionFailure);
        }

        private void Apply(WorkTimeCreated workTimeCreated)
        {
            StartDate = workTimeCreated.StartDate;
            EndDate = workTimeCreated.EndDate;
            DateCreated = workTimeCreated.DateCreated;
            User = workTimeCreated.User;
            AutoStart = workTimeCreated.AutoStart;
        }

        public static WorkTime CreateFromSnapshot(WorkTimeSnapshotCreated snapshotCreated)
        {
            var workTime = new WorkTime();
            workTime.AggregateId = snapshotCreated.AggregateId;
            workTime.AggregateVersion = snapshotCreated.AggregateVersion;
            workTime.FromSnapshot = true;
            workTime.Apply(snapshotCreated);
            return workTime;
        }

        public static WorkTime CreateFromSnapshot(IEnumerable<Event> events)
        {
            var snap = events.First() as WorkTimeSnapshotCreated;
            if (snap == null)
            {
                throw new Exception("First event not snapshot");
            }

            var workTime = CreateFromSnapshot(snap);

            foreach (var ev in events.Skip(1))
            {
                workTime.AsDynamic().Apply(ev);
                workTime.AggregateVersion++;
            }


            return workTime;
        }

        public static WorkTime FromEvents(IEnumerable<Event> events)
        {
            var workTime = new WorkTime();
            Event last = null;
            foreach (var ev in events)
            {
                if (last == null)
                {
                    if (ev.AggregateVersion != 1)
                    {
                        throw new Exception(
                            $"Cannot create aggregate starting from event version {ev.AggregateVersion}");
                    }

                    workTime.AggregateId = ev.AggregateId;
                }

                workTime.AsDynamic().Apply(ev);
                last = ev;
            }

            if (last == null)
            {
                throw new Exception("empty event list");
            }

            workTime.AggregateVersion = last.AggregateVersion;

            return workTime;
        }
    }
}