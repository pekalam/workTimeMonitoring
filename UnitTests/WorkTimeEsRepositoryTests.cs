using System;
using FluentAssertions;
using Infrastructure.Domain;
using Infrastructure.Repositories;
using Xunit;

namespace UnitTests
{
    public abstract class WorkTimeEsRepositoryTests
    {
        protected IWorkTimeEsRepository _repository;

        public WorkTimeEsRepositoryTests()
        {
            _repository = CreateRepository();
        }

        protected abstract IWorkTimeEsRepository CreateRepository();

        [Fact]
        public void Find_when_valid_start_date_finds_full_aggregate()
        {
            var workTime = WorkTimeTestUtils.CreateManual();
            workTime.StartManually();
            workTime.AddMouseAction();
            workTime.AddKeyboardAction();
            
            _repository.Save(workTime);
            workTime.PendingEvents.Count.Should().Be(4);

            workTime.MarkPendingEventsAsHandled();

            var found = _repository.Find(workTime.User, workTime.DateCreated);
            found.Should().NotBeNull();

            found.PendingEvents.Count.Should().Be(0);
            found.Should().BeEquivalentTo(workTime, options =>
            {
                return options.Excluding(time => time.StartDate).Excluding(time => time.EndDate)
                    .Excluding(time => time.ActionEvents);
            });
            found.StartDate.Value.SafeCompare(workTime.StartDate.Value);
            found.EndDate.SafeCompare(workTime.EndDate);
            //todo
            found.ActionEvents.Count.Should().Be(workTime.ActionEvents.Count);
        }

        [Fact]
        public void Rollback_restores_state_to_given_snapshot()
        {
            var workTime = WorkTimeTestUtils.CreateManual();
            workTime.StartManually();
            var snap = workTime.TakeSnapshot();
            workTime.AddMouseAction();
            _repository.Save(workTime);
            workTime.MarkPendingEventsAsHandled();

            _repository.Rollback(snap);
            workTime.RollbackToSnapshot(snap);
            
            var found = _repository.Find(workTime.User, workTime.DateCreated);

            found.Should().BeEquivalentTo(workTime, opt => opt.Excluding(t => t.FromSnapshot));
        }

        [Fact]
        public void f()
        {
            var workTime = WorkTimeTestUtils.CreateManual();
            workTime.StartManually();
            var snap = workTime.TakeSnapshot();
            _repository.Save(workTime);
            workTime.MarkPendingEventsAsHandled();

            workTime.AddMouseAction();

            var prev = _repository.FindFromSnapshot(snap);

            prev.Should().BeEquivalentTo(workTime, opt =>
            {
                return opt.Excluding(w => w.FromSnapshot).Excluding(w => w.PendingEvents).Excluding(w => w.AggregateVersion).Excluding(w => w.ActionEvents);
            });
            prev.FromSnapshot.Should().BeTrue();
            prev.ActionEvents.Should().BeEmpty();
            prev.PendingEvents.Should().BeEmpty();
            prev.AggregateVersion.Should().Be(workTime.AggregateVersion - 1);
        }
    }
}