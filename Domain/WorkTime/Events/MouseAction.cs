using System;

namespace Domain.WorkTime.Events
{
    public class MouseAction : Event
    {
        public MouseAction(int aggregateId, DateTime date, int totalSeconds) : base(aggregateId, date, EventName.MouseAction)
        {
            TotalSeconds = totalSeconds;
        }

        public int TotalSeconds { get; }
    }
}