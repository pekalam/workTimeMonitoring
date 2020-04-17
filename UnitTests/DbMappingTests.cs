using System.Linq;
using AutoMapper;
using DomainTestUtils;
using FluentAssertions;
using Infrastructure.Db;
using Xunit;

namespace Infrastructure.Tests
{
    [Trait("Category", "Integration")]
    public class DbMappingTests
    {
        private IMapper _mapper;

        public DbMappingTests()
        {
            _mapper = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile<DbTestImageProfile>();
                cfg.AddProfile<DbEventProfile>();
            }).CreateMapper();
        }

        [Fact]
        public void f()
        {
            var worktime = WorkTimeTestUtils.CreateManual();

            var ev = worktime.PendingEvents.First();

            var dbEvent = _mapper.Map<DbEvent>(ev);

            dbEvent.Id.Should().BeNull();
            dbEvent.AggregateId.Should().Be(ev.AggregateId);
            dbEvent.AggregateVersion.Should().Be(ev.AggregateVersion);
            dbEvent.Data.Should().NotBeNullOrWhiteSpace();
            dbEvent.Date.Should().Be(ev.Date);
        }
    }
}
