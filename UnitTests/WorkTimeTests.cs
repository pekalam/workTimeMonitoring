using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Accessibility;
using AutoMapper;
using FluentAssertions;
using Infrastructure.Db;
using Infrastructure.Domain;
using Xunit;

namespace UnitTests
{
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

    internal static class DateTimeTestExtentsions
    {
        public static void SafeCompare(this DateTime d1, DateTime d2)
        {
            d1.Subtract(d2).TotalSeconds.Should().BeLessThan(5);
        }
    }

    public static class UserTestUtils
    {
        public static User CreateTestUser()
        {
            return new User(new Username("mpekala"));
        }

    }

    public static class WorkTimeTestUtils
    {
        public static WorkTime CreateManual()
        {
            return new WorkTime(1, UserTestUtils.CreateTestUser(), null, DateTime.UtcNow.AddMinutes(10));
        }

        public static WorkTime CreateStartedManually()
        {
            var workTime = new WorkTime(1, UserTestUtils.CreateTestUser(), null, DateTime.UtcNow.AddMinutes(10));
            workTime.StartManually();
            workTime.MarkPendingEventsAsHandled();
            return workTime;
        }

    }

    public class WorkTimeTests
    {
        private readonly User _user = new User(new Username("mpekala"));

        [Fact]
        public void Start_starts_working_time_and_sets_startDate()
        {
            var workTime = WorkTimeTestUtils.CreateManual();
            workTime.MarkPendingEventsAsHandled();

            workTime.StartDate.Should().BeNull();
            workTime.StartManually();
            workTime.StartDate.Value.SafeCompare(DateTime.UtcNow);
            workTime.StartDate.Value.Kind.Should().Be(DateTimeKind.Utc);
            workTime.User.Should().BeEquivalentTo(_user);
            workTime.PendingEvents.Count.Should().Be(1);
            workTime.PendingEvents.First().Should().BeOfType<WorkTimeStarted>();
            workTime.AggregateVersion.Should().Be(2);
        }


        [Fact]
        public void AddMouseAction_when_not_started_throws()
        {
            var workTime = WorkTimeTestUtils.CreateManual();

            Assert.ThrowsAny<Exception>(() => workTime.AddMouseAction());
        }

        [Fact]
        public void AddMouseAction_when_started_adds_valid_events()
        {
            var workTime = WorkTimeTestUtils.CreateManual();
            workTime.MarkPendingEventsAsHandled();

            workTime.StartManually();
            workTime.MarkPendingEventsAsHandled();
            workTime.AddMouseAction();
            workTime.ActionEvents.Count.Should().Be(1);
            workTime.ActionEvents.First().Should().BeOfType<MouseAction>();
            
            workTime.PendingEvents.Count.Should().Be(1);
            workTime.PendingEvents.First().Should().BeOfType<MouseAction>();
            workTime.AggregateVersion.Should().Be(3);
        }

        [Fact]
        public void AddKeyboardAction_when_not_started_throws()
        {
            var workTime = WorkTimeTestUtils.CreateManual();

            Assert.ThrowsAny<Exception>(() => workTime.AddKeyboardAction());
        }

        [Fact]
        public void Take_snapshot_generates_valid_events_and_clears_actionEvents_list()
        {
            var workTime = WorkTimeTestUtils.CreateManual();

            workTime.StartManually();
            workTime.AddMouseAction();

            workTime.TakeSnapshot();

            workTime.ActionEvents.Count.Should().Be(0);
            workTime.PendingEvents.Count.Should().Be(4);
            workTime.PendingEvents.Last().Should().BeOfType<WorkTimeSnapshotCreated>();
            workTime.AggregateVersion.Should().Be(4);
        }

        [Fact]
        public void FromEvents_recreates_valid_object()
        {
            var now = DateTime.UtcNow;
            var workTime = WorkTimeTestUtils.CreateManual();

            workTime.StartManually();
            workTime.AddMouseAction();
            workTime.AddKeyboardAction();


            var recreated = WorkTime.FromEvents(workTime.PendingEvents);
            workTime.MarkPendingEventsAsHandled();

            recreated.Should().BeEquivalentTo(workTime, options =>
            {
                return options.Excluding(time => time.StartDate).Excluding(time => time.EndDate).Excluding(time => time.DateCreated);
            });
            recreated.StartDate.Value.SafeCompare(workTime.StartDate.Value);
            recreated.EndDate.SafeCompare(workTime.EndDate);
            recreated.DateCreated.SafeCompare(now);
        }

        [Fact]
        public void RollbackToSnapshot_discards_changes()
        {
            var workTime = WorkTimeTestUtils.CreateManual();

            workTime.StartManually();
            workTime.AddMouseAction();
            workTime.AddKeyboardAction();

            var snap = workTime.TakeSnapshot();
            workTime.MarkPendingEventsAsHandled();

            workTime.AddMouseAction();

            workTime.AggregateVersion.Should().Be(6);
            workTime.RollbackToSnapshot(snap);
            workTime.PendingEvents.Count.Should().Be(0);
            workTime.ActionEvents.Count.Should().Be(0);
            workTime.AggregateVersion.Should().Be(5);
            workTime.FromSnapshot.Should().BeTrue();
        }
    }
}
