using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Media;
using AutoMapper;
using Infrastructure.Domain;
using Newtonsoft.Json;

namespace Infrastructure.Db
{
    internal class DbEvent
    {
        public long? Id { get; set; }
        public int EventName { get; set; }
        public int AggregateId { get; set; }
        public long AggregateVersion { get; set; }
        public DateTime Date { get; set; }
        public string Data { get; set; }
    }

    internal class DbEventProfile : Profile
    {
        public DbEventProfile()
        {
            CreateMap<Event, DbEvent>()
                .ForMember(db => db.EventName, opt => opt.MapFrom<int>(ev => (int)ev.EventName))
                .ForMember(db => db.Data, opt => opt.MapFrom<string>(ev => DbEventSerializer.Serialize(ev)));

            CreateMap<DbEvent, Event>()
                .ConvertUsing<DbEventConverter>();
        }
    }

    internal class DbEventConverter : ITypeConverter<DbEvent, Event>
    {
        public Event Convert(DbEvent source, Event destination, ResolutionContext context)
        {
            var ev = DbEventSerializer.Deserialize(source.Data);
            ev.AggregateId = source.AggregateId;
            ev.AggregateVersion = source.AggregateVersion;
            ev.EventName = (EventName)source.EventName;
            ev.Id = source.Id;

            return ev;
        }
    }

    internal static class DbEventMapperExtensions
    {
        public static IEnumerable<Event> MapToEvents(this IEnumerable<DbEvent> dbevents, IMapper mapper)
        {
            return dbevents.Select(mapper.Map<DbEvent, Event>);
        }
    }

    internal static class DbEventSerializer
    {
        public static Event Deserialize(string json)
        {
            var ev = JsonConvert.DeserializeObject(json, new JsonSerializerSettings()
            {
                TypeNameHandling = TypeNameHandling.All,
            }) as Event;

            if (ev == null)
            {
                throw new Exception($"Event deserialization error: {json}");
            }

            return ev;
        }

        public static string Serialize(Event ev)
        {
            var json = JsonConvert.SerializeObject(ev, new JsonSerializerSettings()
            {
                TypeNameHandling = TypeNameHandling.All,
                DateFormatString = "yyyy'-'MM'-'dd' 'HH':'mm':'ss.FFFFFFFK",
            });

            if (string.IsNullOrWhiteSpace(json))
            {
                throw new Exception("Event serialization error");
            }

            return json;
        }
    }
}
