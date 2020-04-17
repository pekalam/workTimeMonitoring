using System;

namespace Domain.WorkTime.Events
{
    public class KeyboardAction : Event 
    {
        public KeyboardAction(int aggregateId, DateTime date, int totalSeconds) : base(aggregateId, date, EventName.KeyboardAction)
        {
            TotalSeconds = totalSeconds;
        }

        public int TotalSeconds { get; }
    }
}