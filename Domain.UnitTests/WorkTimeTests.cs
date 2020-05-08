using Domain.User;
using Domain.WorkTimeAggregate;
using Domain.WorkTimeAggregate.Events;
using DomainTestUtils;
using FluentAssertions;
using System;
using System.Linq;
using Xunit;

namespace Domain.UnitTests
{
    public class WorkTimeTests
    {
        private readonly User.User _user = new User.User(1, new Username("mpekala"));
        private MouseKeyboardEvent _testMkEvent = new MouseKeyboardEvent()
        {
            Start = InternalTimeService.GetCurrentDateTime(),
            End = InternalTimeService.GetCurrentDateTime().AddMinutes(1),
        };

        public WorkTimeTests()
        {
            InternalTimeService.GetCurrentDateTime = () => DateTime.UtcNow;
        }

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

            workTime.Started.Should().BeTrue();
            workTime.Stopped.Should().BeFalse();
            workTime.Paused.Should().BeFalse();
            workTime.StoppedByUser.Should().BeFalse();
        }

        [Fact]
        public void Stop_if_called_stops_workTIme()
        {
            var workTime = WorkTimeTestUtils.CreateManual(_user);
            workTime.MarkPendingEventsAsHandled();
            workTime.StartManually();

            workTime.Stop();

            workTime.Started.Should().BeTrue();
            workTime.Stopped.Should().BeTrue();
            workTime.StoppedByUser.Should().BeTrue();
            workTime.Paused.Should().BeFalse();
        }

        [Fact]
        public void When_past_end_time_workTime_is_stopped()
        {
            var workTime = WorkTimeTestUtils.CreateManual(_user);
            workTime.MarkPendingEventsAsHandled();
            workTime.StartManually();

            InternalTimeService.GetCurrentDateTime = () => DateTime.UtcNow.AddMinutes(11);

            workTime.Started.Should().BeTrue();
            workTime.Stopped.Should().BeTrue();
            workTime.StoppedByUser.Should().BeFalse();
            workTime.Paused.Should().BeFalse();
        }

        [Fact]
        public void set_interrupted_stopped_or_paused__does_not_gen_event()
        {
            var workTime = WorkTimeTestUtils.CreateManual(_user);
            workTime.StartManually();
            workTime.MarkPendingEventsAsHandled();

            workTime.Pause();
            workTime.SetInterrupted();
            workTime.Resume();

            InternalTimeService.GetCurrentDateTime = () => DateTime.UtcNow.AddMinutes(11);

            workTime.SetInterrupted();
            workTime.PendingEvents.Count.Should().Be(0);
        }

        [Fact]
        public void Combine_ads_new_events()
        {
            var workTime = WorkTimeTestUtils.CreateManual(_user);
            workTime.StartManually();
            var snap = workTime.TakeSnapshot();

            var totalEvs = workTime.PendingEvents.Count;

            var fromSnap = WorkTime.CreateFromSnapshot(snap);
            fromSnap.AddRecognitionFailure(DateTime.UtcNow, false, false);

            WorkTime joined = WorkTime.Combine(workTime, fromSnap);

            joined.PendingEvents.Count.Should().Be( totalEvs + 1);
            joined.PendingEvents.Last().Should().BeOfType<FaceRecognitionFailure>();
        }

        [Fact]
        public void SetResored_when_interrupted_generates_event()
        {
            var workTime = WorkTimeTestUtils.CreateManual(_user);
            workTime.StartManually();
            workTime.MarkPendingEventsAsHandled();
            
            workTime.SetInterrupted();

            workTime.PendingEvents.Last().Should().BeOfType<WorkTimeInterrupted>();

            InternalTimeService.GetCurrentDateTime = () => DateTime.UtcNow.AddMilliseconds(2000);

            workTime.SetRestored();

            workTime.PendingEvents.Last().Should().BeOfType<WorkTimeRestored>();

            var restoredEv = workTime.PendingEvents.Last() as WorkTimeRestored;

            restoredEv.TotalTimeMs.Should().BeInRange(2000, 2100);

            //not throwing
            workTime.AddRecognitionFailure(InternalTimeService.GetCurrentDateTime(), true, false);
        }

        [Fact]
        public void SetResored_when_not_interrupted_generates_event()
        {
            var workTime = WorkTimeTestUtils.CreateManual(_user);
            workTime.StartManually();
            workTime.MarkPendingEventsAsHandled();
            workTime.SetRestored();

            workTime.PendingEvents.Last().Should().BeOfType<WorkTimeRestored>();

            var restoredEv = workTime.PendingEvents.Last() as WorkTimeRestored;
            restoredEv.TotalTimeMs.Should().Be(0);
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
            InternalTimeService.GetCurrentDateTime = () => DateTime.UtcNow.AddMinutes(1);
            workTime.AddRecognitionFailure(DateTime.UtcNow, true, false);

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
            InternalTimeService.GetCurrentDateTime = () => DateTime.UtcNow.AddMinutes(1);
            workTime.AddRecognitionFailure(DateTime.UtcNow, true, false);


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
            workTime.UserWatchingScreen.Count.Should().Be(0);
            workTime.FaceRecognitionFailures.Count.Should().Be(0);
            workTime.AggregateVersion.Should().Be(5);
            workTime.FromSnapshot.Should().BeTrue();
        }
    }


}