using System;

namespace Domain.WorkTimeAggregate.Events
{
    public class KeyboardAction : Event 
    {
        public KeyboardAction(int aggregateId, DateTime date, MouseKeyboardEvent mkEvent) : base(aggregateId, date, EventName.KeyboardAction)
        {
            MkEvent = mkEvent;
        }

        public MouseKeyboardEvent MkEvent { get; }
    }
}