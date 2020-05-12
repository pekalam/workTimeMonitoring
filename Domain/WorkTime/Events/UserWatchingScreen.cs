using System;

namespace Domain.WorkTimeAggregate.Events
{
    public class UserWatchingScreen : Event
    {
        public UserWatchingScreen(long aggregateId, DateTime date, long totalTimeMs, string executable) : base(aggregateId, date, EventName.UserWatchingScreen)
        {
            TotalTimeMs = totalTimeMs;
            Executable = executable;
        }

        public long TotalTimeMs { get; }
        public string Executable { get; }
    }
}
