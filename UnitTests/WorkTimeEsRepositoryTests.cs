using Domain;
using Domain.Repositories;
using DomainTestUtils;
using FluentAssertions;
using Xunit;

namespace Infrastructure.Tests
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

            for (int i = 0; i < found.ActionEvents.Count; i++)
            {
                found.ActionEvents[i].Should().BeEquivalentTo(workTime.ActionEvents[i], 
                    opt => opt
                        .Excluding(e => e.Date).Excluding(e => e.Id));
                found.ActionEvents[i].Id.Should().NotBeNull();
                found.ActionEvents[i].Date.SafeCompare(workTime.ActionEvents[i].Date);
            }
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
        public void FindFromSnapshot_returns_aggregate_from_snapshot()
        {
            var workTime = WorkTimeTestUtils.CreateManual();
            workTime.StartManually();
            var snap = workTime.TakeSnapshot();

            _repository.Save(workTime);
            workTime.MarkPendingEventsAsHandled();

            workTime.AddMouseAction();

            var fromSnap = _repository.FindFromSnapshot(snap);

            fromSnap.Should().BeEquivalentTo(workTime, opt =>
            {
                return opt.Excluding(w => w.FromSnapshot).Excluding(w => w.PendingEvents).Excluding(w => w.AggregateVersion).Excluding(w => w.ActionEvents);
            });
            fromSnap.FromSnapshot.Should().BeTrue();
            fromSnap.ActionEvents.Should().BeEmpty();
            fromSnap.PendingEvents.Should().BeEmpty();
            fromSnap.AggregateVersion.Should().Be(workTime.AggregateVersion - 1);
        }
    }
}