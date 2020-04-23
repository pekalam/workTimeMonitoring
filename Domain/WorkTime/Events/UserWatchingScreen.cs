using System;
using System.Collections.Generic;
using System.Text;
using Domain.WorkTimeAggregate.Events;

namespace Domain.WorkTimeAggregate.Events
{
    public class UserWatchingScreen : Event
    {
        public UserWatchingScreen(long aggregateId, DateTime date, DateTime endDate, long totalTimeMs) : base(aggregateId, date, EventName.UserWatchingScreen)
        {
            EndDate = endDate;
            TotalTimeMs = totalTimeMs;
        }

        public DateTime EndDate { get; }
        public long TotalTimeMs { get; }
    }
}
