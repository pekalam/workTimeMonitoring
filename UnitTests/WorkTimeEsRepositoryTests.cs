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

        protected WorkTime CreateStartedWorkTime()
        {
            var w = new WorkTime();
            w.Start(new User(new Username("mpekala")));
            return w;
        }

        [Fact]
        public void Find_when_valid_start_date_finds_full_aggregate()
        {
            var workTime = CreateStartedWorkTime();
            workTime.AddMouseAction();
            workTime.AddKeyboardAction();
            
            _repository.Save(workTime);
            workTime.PendingEvents.Count.Should().Be(3);

            workTime.MarkPendingEventsAsHandled();

            var found = _repository.Find(workTime.User, workTime.StartDate, DateTime.MaxValue);
            found.Should().NotBeNull();

            found.PendingEvents.Count.Should().Be(0);
            found.Should().BeEquivalentTo(workTime, options =>
            {
                return options.Excluding(time => time.StartDate).Excluding(time => time.EndDate)
                    .Excluding(time => time.ActionEvents);
            });
            found.StartDate.SafeCompare(workTime.StartDate);
            found.EndDate.SafeCompare(workTime.EndDate);
            //todo
            found.ActionEvents.Count.Should().Be(workTime.ActionEvents.Count);
        }

        [Fact]
        public void Rollback_restores_state_to_given_snapshot()
        {
            var workTime = CreateStartedWorkTime();
            workTime.AddMouseAction();
            var snap = workTime.TakeSnapshot();
            _repository.Save(workTime);
            workTime.MarkPendingEventsAsHandled();


        }
    }
}