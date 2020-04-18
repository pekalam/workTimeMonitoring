using System;
using System.Runtime.CompilerServices;

[assembly:InternalsVisibleTo("DomainTestUtils")]
namespace Domain.WorkTimeAggregate.Events
{
    public class Event
    {
        public long? Id { get; internal set; }
        public int AggregateId { get; internal set; }
        public long AggregateVersion { get; internal set; }
        public DateTime Date { get; internal set; }
        public EventName EventName { get; internal set; }

        internal Event()
        {
        }

        public Event(int aggregateId, DateTime date, EventName eventName)
        {
            AggregateId = aggregateId;
            Date = date;
            EventName = eventName;
        }
    }

    public static class EventDeserializationHelper
    {
        public static void Deserialize(long? id, int aggregateId, long aggregateVersion, DateTime date,
            EventName eventName, Event ev)
        {
            ev.Id = id;
            ev.AggregateId = aggregateId;
            ev.AggregateVersion = aggregateVersion;
            ev.EventName = eventName;
            ev.Date = date;
        }
    }
}