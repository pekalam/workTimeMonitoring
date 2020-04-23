using System;
using System.Collections.Generic;
using System.Text;
using Domain.WorkTimeAggregate.Events;

namespace Domain.WorkTimeAggregate.Events
{
    /// <summary>
    /// Implicit event
    /// </summary>
    public class UserWatchingScreen : Event
    {
        public UserWatchingScreen(long aggregateId, DateTime date, EventName eventName, long totalTimeMs) : base(aggregateId, date, eventName)
        {
            TotalTimeMs = totalTimeMs;
        }

        public long TotalTimeMs { get; }
    }

    /// <summary>
    /// Implicit event
    /// </summary>
    public class UserWorking : Event
    {
        public UserWorking(long aggregateId, DateTime date, EventName eventName, DateTime endDate) : base(aggregateId, date, eventName)
        {
            EndDate = endDate;
        }

        public DateTime EndDate { get; }
    }
}
