using System;

namespace Domain.WorkTimeAggregate.Events
{
    public class WorkTimeStarted : Event
    {
        public WorkTimeStarted(long aggregateId, DateTime date, DateTime startDate) : base(aggregateId, date, EventName.WorkTimeStarted)
        {
            StartDate = startDate;
        }

        public DateTime StartDate { get; }
    }
}