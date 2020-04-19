using System;

namespace Domain.WorkTimeAggregate.Events
{
    public class KeyboardAction : Event 
    {
        public KeyboardAction(long aggregateId, DateTime date, MouseKeyboardEvent mkEvent) : base(aggregateId, date, EventName.KeyboardAction)
        {
            MkEvent = mkEvent;
        }

        public MouseKeyboardEvent MkEvent { get; }
    }
}