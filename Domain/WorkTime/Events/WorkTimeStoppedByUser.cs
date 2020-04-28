using System;
using System.Collections.Generic;
using System.Text;
using Domain.WorkTimeAggregate.Events;

namespace Domain.WorkTimeAggregate.Events
{
    public class WorkTimeStoppedByUser : Event
    {
        public WorkTimeStoppedByUser(long aggregateId, DateTime date) : base(aggregateId, date, EventName.StoppedByUser)
        {
        }
    }
}
