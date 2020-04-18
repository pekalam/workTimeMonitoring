using System;
using Domain;
using Domain.Repositories;
using Domain.WorkTimeAggregate;
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
        public void Save_adds_new()
        {
            var workTime = WorkTimeTestUtils.CreateManual();
            _repository.Save(workTime);

            _repository.CountForUser(workTime.User).Should().Be(1);
        }

        [Fact]
        public void Save_called_twice_with_same_param_throws()
        {
            var workTime = WorkTimeTestUtils.CreateManual();
            _repository.Save(workTime);
            _repository.CountForUser(workTime.User).Should().Be(1);

            Assert.ThrowsAny<Exception>(() => _repository.Save(workTime));

            _repository.CountForUser(workTime.User).Should().Be(1);
        }

        [Fact]
        public void CountForUser_returns_valid_num()
        {

        }

        [Fact]
        public void Find_when_valid_start_date_finds_full_aggregate()
        {
            var workTime = WorkTimeTestUtils.CreateManual();
            workTime.StartManually();
            workTime.AddMouseAction(new MouseKeyboardEvent());
            workTime.AddKeyboardAction(new MouseKeyboardEvent());
            
            _repository.Save(workTime);
            workTime.PendingEvents.Count.Should().Be(4);

            workTime.MarkPendingEventsAsHandled();

            var found = _repository.Find(workTime.User, workTime.DateCreated);

            found.PendingEvents.Count.Should().Be(0);
            found.Should().BeEquivalentTo(workTime, options =>
            {
                return options.Excluding(time => time.StartDate).Excluding(time => time.EndDate)
                    .Excluding(t => t.MouseActionEvents)
                    .Excluding(t => t.KeyboardActionEvents);
            });
            found.StartDate.Value.SafeCompare(workTime.StartDate.Value);
            found.EndDate.SafeCompare(workTime.EndDate);
            //todo
            found.MouseActionEvents.Count.Should().Be(workTime.MouseActionEvents.Count);

            for (int i = 0; i < found.MouseActionEvents.Count; i++)
            {
                found.MouseActionEvents[i].Should().BeEquivalentTo(workTime.MouseActionEvents[i], 
                    opt => opt
                        .Excluding(e => e.Date).Excluding(e => e.Id));
                found.MouseActionEvents[i].Id.Should().NotBeNull();
                found.MouseActionEvents[i].Date.SafeCompare(workTime.MouseActionEvents[i].Date);
            }


            found.KeyboardActionEvents.Count.Should().Be(workTime.KeyboardActionEvents.Count);

            for (int i = 0; i < found.KeyboardActionEvents.Count; i++)
            {
                found.KeyboardActionEvents[i].Should().BeEquivalentTo(workTime.KeyboardActionEvents[i],
                    opt => opt
                        .Excluding(e => e.Date).Excluding(e => e.Id));
                found.KeyboardActionEvents[i].Id.Should().NotBeNull();
                found.KeyboardActionEvents[i].Date.SafeCompare(workTime.KeyboardActionEvents[i].Date);
            }
        }

        [Fact]
        public void Rollback_restores_state_to_given_snapshot()
        {
            var workTime = WorkTimeTestUtils.CreateManual();
            workTime.StartManually();
            var snap = workTime.TakeSnapshot();
            workTime.AddMouseAction(new MouseKeyboardEvent());
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

            workTime.AddMouseAction(new MouseKeyboardEvent());

            var fromSnap = _repository.FindFromSnapshot(snap);

            fromSnap.Should().BeEquivalentTo(workTime, opt =>
            {
                return opt.Excluding(w => w.FromSnapshot).Excluding(w => w.PendingEvents).Excluding(w => w.AggregateVersion)
                    .Excluding(t => t.MouseActionEvents).Excluding(t => t.KeyboardActionEvents);
            });
            fromSnap.FromSnapshot.Should().BeTrue();
            fromSnap.MouseActionEvents.Should().BeEmpty();
            fromSnap.KeyboardActionEvents.Should().BeEmpty();
            fromSnap.PendingEvents.Should().BeEmpty();
            fromSnap.AggregateVersion.Should().Be(workTime.AggregateVersion - 1);
        }
    }
}