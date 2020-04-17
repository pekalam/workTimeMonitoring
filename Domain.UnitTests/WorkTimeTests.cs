using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Domain.User;
using Domain.WorkTime.Events;
using DomainTestUtils;
using FluentAssertions;
using Moq;
using OpenCvSharp;
using WorkTimeAlghorithm;
using Xunit;

namespace Domain.UnitTests
{
    public class WorkTimeTests
    {
        private readonly User.User _user = new User.User(new Username("mpekala"));

        [Fact]
        public void Start_starts_working_time_and_sets_startDate()
        {
            var workTime = WorkTimeTestUtils.CreateManual();
            workTime.MarkPendingEventsAsHandled();

            workTime.StartDate.Should().BeNull();
            workTime.StartManually();
            DateTimeTestExtentsions.SafeCompare(workTime.StartDate.Value, DateTime.UtcNow);
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


            var recreated = WorkTime.WorkTime.FromEvents(workTime.PendingEvents);
            workTime.MarkPendingEventsAsHandled();

            recreated.Should().BeEquivalentTo(workTime, options =>
            {
                return options.Excluding(time => time.StartDate).Excluding(time => time.EndDate).Excluding(time => time.DateCreated);
            });
            DateTimeTestExtentsions.SafeCompare(recreated.StartDate.Value, workTime.StartDate.Value);
            DateTimeTestExtentsions.SafeCompare(recreated.EndDate, workTime.EndDate);
            DateTimeTestExtentsions.SafeCompare(recreated.DateCreated, now);
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