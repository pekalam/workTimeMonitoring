using System;

namespace Domain.WorkTimeAggregate.Events
{
    public class WorkTimeSnapshotCreated : Event
    {
        public WorkTimeSnapshotCreated(long aggregateId, DateTime date, WorkTimeSnapshot snapshot) : base(aggregateId, date, EventName.WorkTimeSnapshotCreated)
        {
            Snapshot = snapshot;
        }

        public WorkTimeSnapshot Snapshot { get; }
    }
}