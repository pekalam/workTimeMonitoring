using System;

namespace Domain.WorkTimeAggregate.Events
{
    public class WorkTimeInterrupted : Event
    {
        public WorkTimeInterrupted(long aggregateId, DateTime date) : base(aggregateId, date, EventName.WorkTimeInterrupted)
        {
        }
    }

    public class WorkTimeRestored : Event
    {
        public WorkTimeRestored(long aggregateId, DateTime date, long totalTimeMs) : base(aggregateId, date, EventName.WorkTimeRestored)
        {
            TotalTimeMs = totalTimeMs;
        }

        public long TotalTimeMs { get; }
    }
}
