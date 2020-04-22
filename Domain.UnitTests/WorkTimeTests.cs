using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Domain.User;
using Domain.WorkTimeAggregate;
using Domain.WorkTimeAggregate.Events;
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
        private readonly User.User _user = new User.User(1, new Username("mpekala"));
        private MouseKeyboardEvent _testMkEvent = new MouseKeyboardEvent();

        [Fact]
        public void Start_starts_working_time_and_sets_startDate()
        {
            var workTime = WorkTimeTestUtils.CreateManual(_user);
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

            Assert.ThrowsAny<Exception>(() => workTime.AddMouseAction(_testMkEvent));
        }

        [Fact]
        public void AddMouseAction_when_started_adds_valid_events()
        {
            var workTime = WorkTimeTestUtils.CreateManual();
            workTime.MarkPendingEventsAsHandled();

            workTime.StartManually();
            workTime.MarkPendingEventsAsHandled();
            workTime.AddMouseAction(_testMkEvent);
            workTime.MouseActionEvents.Count.Should().Be(1);
            workTime.MouseActionEvents.First().Should().BeOfType<MouseAction>();
            
            workTime.PendingEvents.Count.Should().Be(1);
            workTime.PendingEvents.First().Should().BeOfType<MouseAction>();
            workTime.AggregateVersion.Should().Be(3);
        }

        [Fact]
        public void AddKeyboardAction_when_not_started_throws()
        {
            var workTime = WorkTimeTestUtils.CreateManual();

            Assert.ThrowsAny<Exception>(() => workTime.AddKeyboardAction(_testMkEvent));
        }

        [Fact]
        public void Take_snapshot_generates_valid_events_and_clears_actionEvents_list()
        {
            var workTime = WorkTimeTestUtils.CreateManual();

            workTime.StartManually();
            workTime.AddMouseAction(_testMkEvent);
            workTime.AddKeyboardAction(_testMkEvent);
            workTime.AddRecognitionFailure(true, false);

            workTime.TakeSnapshot();

            workTime.KeyboardActionEvents.Count.Should().Be(0);
            workTime.MouseActionEvents.Count.Should().Be(0);
            workTime.FaceRecognitionFailures.Count.Should().Be(0);
            workTime.PendingEvents.Count.Should().Be(6);
            workTime.PendingEvents.Last().Should().BeOfType<WorkTimeSnapshotCreated>();
            workTime.AggregateVersion.Should().Be(6);
        }

        [Fact]
        public void FromEvents_recreates_valid_object()
        {
            var now = DateTime.UtcNow;
            var workTime = WorkTimeTestUtils.CreateManual();

            workTime.StartManually();
            workTime.AddMouseAction(_testMkEvent);
            workTime.AddKeyboardAction(_testMkEvent);
            workTime.AddRecognitionFailure(true, false);


            var recreated = WorkTimeAggregate.WorkTime.FromEvents(workTime.PendingEvents);
            workTime.MarkPendingEventsAsHandled();

            recreated.Should().BeEquivalentTo(workTime, options =>
            {
                return options.Excluding(time => time.StartDate)
                    .Excluding(t => t.KeyboardActionEvents)
                    .Excluding(t => t.MouseActionEvents)
                    .Excluding(t => t.FaceRecognitionFailures)
                    .Excluding(time => time.EndDate)
                    .Excluding(time => time.DateCreated);
            });
            recreated.StartDate.Value.SafeCompare(workTime.StartDate.Value);
            recreated.EndDate.SafeCompare(workTime.EndDate);
            recreated.DateCreated.SafeCompare(now);

            recreated.KeyboardActionEvents.Count.Should().Be(1);
            recreated.MouseActionEvents.Count.Should().Be(1);
            recreated.FaceRecognitionFailures.Count.Should().Be(1);
        }

        [Fact]
        public void RollbackToSnapshot_discards_changes()
        {
            var workTime = WorkTimeTestUtils.CreateManual();

            workTime.StartManually();
            workTime.AddMouseAction(_testMkEvent);
            workTime.AddKeyboardAction(_testMkEvent);

            var snap = workTime.TakeSnapshot();
            workTime.MarkPendingEventsAsHandled();

            workTime.AddMouseAction(_testMkEvent);

            workTime.AggregateVersion.Should().Be(6);
            workTime.RollbackToSnapshot(snap);
            workTime.PendingEvents.Count.Should().Be(0);
            workTime.MouseActionEvents.Count.Should().Be(0);
            workTime.KeyboardActionEvents.Count.Should().Be(0);
            workTime.AggregateVersion.Should().Be(5);
            workTime.FromSnapshot.Should().BeTrue();
        }
    }


}