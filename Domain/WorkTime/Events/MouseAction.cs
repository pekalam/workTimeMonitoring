using System;

namespace Domain.WorkTimeAggregate.Events
{
    public class MouseAction : Event
    {
        public MouseAction(int aggregateId, DateTime date, MouseKeyboardEvent mkEvent) : base(aggregateId, date, EventName.MouseAction)
        {
            MkEvent = mkEvent;
        }

        public MouseKeyboardEvent MkEvent { get; }
    }
}