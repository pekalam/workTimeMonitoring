using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Accessibility;
using FluentAssertions;
using Infrastructure.Domain;
using Xunit;

namespace UnitTests
{
    internal static class DateTimeTestExtentsions
    {
        public static void SafeCompare(this DateTime d1, DateTime d2)
        {
            d1.Subtract(d2).TotalSeconds.Should().BeLessThan(5);
        }
    }

    public class WorkTimeTests
    {
        private readonly WorkTime _workTime = new WorkTime();
        private readonly User _user = new User(new Username("mpekala"));

            [Fact]
        public void Start_starts_working_time_and_sets_startDate()
        {
            _workTime.StartDate.Should().Be(default(DateTime));
            _workTime.Start(_user);
            _workTime.StartDate.Subtract(DateTime.UtcNow).TotalSeconds.Should().BeLessThan(5);
            _workTime.StartDate.Kind.Should().Be(DateTimeKind.Utc);
            _workTime.User.Should().BeEquivalentTo(_user);
            _workTime.PendingEvents.Count.Should().Be(1);
            _workTime.PendingEvents.First().Should().BeOfType<WorkTimeStarted>();
            _workTime.AggregateVersion.Should().Be(1);
        }


        [Fact]
        public void AddMouseAction_when_not_started_throws()
        {
            Assert.ThrowsAny<Exception>(() => _workTime.AddMouseAction());
        }

        [Fact]
        public void AddMouseAction_when_started_adds_valid_events()
        {
            _workTime.Start(_user);
            _workTime.MarkPendingEventsAsHandled();
            _workTime.AddMouseAction();
            _workTime.ActionEvents.Count.Should().Be(1);
            _workTime.ActionEvents.First().Should().BeOfType<MouseAction>();
            
            _workTime.PendingEvents.Count.Should().Be(1);
            _workTime.PendingEvents.First().Should().BeOfType<MouseAction>();
            _workTime.AggregateVersion.Should().Be(2);
        }

        [Fact]
        public void AddKeyboardAction_when_not_started_throws()
        {
            Assert.ThrowsAny<Exception>(() => _workTime.AddKeyboardAction());
        }

        [Fact]
        public void Take_snapshot_generates_valid_events_and_clears_actionEvents_list()
        {
            _workTime.Start(_user);
            _workTime.AddMouseAction();

            _workTime.TakeSnapshot();

            _workTime.ActionEvents.Count.Should().Be(0);
            _workTime.PendingEvents.Count.Should().Be(3);
            _workTime.PendingEvents.Last().Should().BeOfType<WorkTimeSnapshotCreated>();
            _workTime.AggregateVersion.Should().Be(3);
        }

        [Fact]
        public void FromEvents_recreates_valid_object()
        {
            _workTime.Start(_user);
            _workTime.AddMouseAction();
            _workTime.AddKeyboardAction();


            var recreated = WorkTime.FromEvents(_workTime.PendingEvents);
            _workTime.MarkPendingEventsAsHandled();

            recreated.Should().BeEquivalentTo(_workTime, options => { return options.Excluding(time => time.StartDate).Excluding(time => time.EndDate); });
            recreated.StartDate.SafeCompare(_workTime.StartDate);
            recreated.EndDate.SafeCompare(_workTime.EndDate);
        }

        [Fact]
        public void RollbackToSnapshot_discards_changes()
        {
            _workTime.Start(_user);
            _workTime.AddMouseAction();
            _workTime.AddKeyboardAction();

            var snap = _workTime.TakeSnapshot();
            _workTime.MarkPendingEventsAsHandled();

            _workTime.AddMouseAction();

            _workTime.AggregateVersion.Should().Be(5);
            _workTime.RollbackToSnapshot(snap);
            _workTime.PendingEvents.Count.Should().Be(0);
            _workTime.ActionEvents.Count.Should().Be(0);
            _workTime.AggregateVersion.Should().Be(4);
        }
    }
}
