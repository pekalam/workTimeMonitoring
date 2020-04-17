using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Windows.Media;
using AutoMapper;
using Domain;
using Domain.WorkTime.Events;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

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
                .ForMember(db => db.EventName, opt => opt.MapFrom<int>(ev => (int) ev.EventName))
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
            EventDeserializationHelper.Deserialize(source.Id, source.AggregateId, source.AggregateVersion, source.Date,
                (EventName)source.EventName, ev);
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

    internal class EventContractResolver : DefaultContractResolver
    {
        private readonly string[] _ignored;

        public EventContractResolver(params string[] ignored)
        {
            _ignored = ignored;
        }


        protected override JsonProperty CreateProperty(MemberInfo member, MemberSerialization memberSerialization)
        {
            var prop = base.CreateProperty(member, memberSerialization);

            if (_ignored.Contains(prop.PropertyName))
            {
                prop.ShouldDeserialize = _ => false;
            }

            return prop;
        }
    }

    internal static class DbEventSerializer
    {
        private static readonly EventContractResolver ContractResolver;

        static DbEventSerializer()
        {
            ContractResolver = new EventContractResolver(
                nameof(Event.Id),
                nameof(Event.AggregateId),
                nameof(Event.AggregateVersion),
                nameof(Event.Date),
                nameof(Event.EventName));
        }

        public static Event Deserialize(string json)
        {
            var ev = JsonConvert.DeserializeObject(json, new JsonSerializerSettings()
            {
                TypeNameHandling = TypeNameHandling.All,
                ConstructorHandling = ConstructorHandling.AllowNonPublicDefaultConstructor,
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
                ContractResolver = ContractResolver,
            });

            if (string.IsNullOrWhiteSpace(json))
            {
                throw new Exception("Event serialization error");
            }

            return json;
        }
    }
}