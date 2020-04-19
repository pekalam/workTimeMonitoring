using System;

namespace Domain.WorkTimeAggregate.Events
{
    public class WorkTimeCreated : Event
    {
        public WorkTimeCreated(long aggregateId, DateTime date, DateTime? startDate, DateTime endDate, DateTime dateCreated, User.User user, bool autoStart) : base(aggregateId, date, EventName.WorkTimeCreated)
        {
            StartDate = startDate;
            EndDate = endDate;
            DateCreated = dateCreated;
            User = user;
            AutoStart = autoStart;
        }

        public DateTime? StartDate { get; }
        public DateTime EndDate { get; set; }
        public DateTime DateCreated { get; set; }
        public User.User User { get; }
        public bool AutoStart { get; }
    }
}