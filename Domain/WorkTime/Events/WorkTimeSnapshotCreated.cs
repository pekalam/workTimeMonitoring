using System;

namespace Domain.WorkTime.Events
{
    public class WorkTimeSnapshotCreated : Event
    {
        public WorkTimeSnapshotCreated(int aggregateId, DateTime date, WorkTimeSnapshot snapshot) : base(aggregateId, date, EventName.WorkTimeSnapshotCreated)
        {
            Snapshot = snapshot;
        }

        public WorkTimeSnapshot Snapshot { get; }
    }
}