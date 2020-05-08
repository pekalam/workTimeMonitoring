using System;

namespace Domain.WorkTimeAggregate.Events
{
    public class UserWatchingScreen : Event
    {
        public UserWatchingScreen(long aggregateId, DateTime date, DateTime endDate, long totalTimeMs, string executable) : base(aggregateId, date, EventName.UserWatchingScreen)
        {
            EndDate = endDate;
            TotalTimeMs = totalTimeMs;
            Executable = executable;
        }

        public DateTime EndDate { get; }
        public long TotalTimeMs { get; }
        public string Executable { get; }
    }
}
