using System;
using System.Windows.Forms;
using Domain.Repositories;
using Domain.Services;
using Domain.User;
using Domain.WorkTimeAggregate;
using Domain.WorkTimeAggregate.Events;
using DomainTestUtils;
using FluentAssertions;
using Infrastructure.src.Services;
using Xunit;

namespace Infrastructure.Tests
{
    public abstract class WorkTimeEventServiceTests
    {
        private WorkTimeEventService _service;
        private WorkTimeBuildService _buildService;
        private IWorkTimeEsRepository _repository;

        public WorkTimeEventServiceTests()
        {
            var repo = CreateRepo();
            var idGen = CreateIdGen();

            _service = new WorkTimeEventService(new WorkTimeUow(repo), repo, TestUtils.ConfigurationService);
            _buildService = new WorkTimeBuildService(repo, idGen);
            _repository = repo;
        }

        protected abstract IWorkTimeUow CreateUow();
        protected abstract IWorkTimeEsRepository CreateRepo();
        protected abstract IWorkTimeIdGeneratorService CreateIdGen();

        [Fact]
        public void f()
        {
            var workTime =
                _buildService.CreateStartedManually(UserTestUtils.CreateTestUser(), DateTime.UtcNow.AddMinutes(60));
            _service.SetWorkTime(workTime);
            _service.MouseEventBufferSz = 0;
            _service.KeyboardEventBufferSz = 0;
            workTime.StartManually();

            _service.AddKeyboardEvent(new MonitorEvent()
            {
                EventStart = DateTime.UtcNow,
                TotalTimeMs = 200,
            });

            _service.AddMouseEvent(new MonitorEvent()
            {
                EventStart = DateTime.UtcNow,
                TotalTimeMs = 500
            });

            _service.AddKeyboardEvent(new MonitorEvent()
            {
                EventStart = DateTime.UtcNow.AddSeconds(65),
                TotalTimeMs = 200,
            });
            workTime.PendingEvents.Count.Should().Be(0);

            _service.AddMouseEvent(new MonitorEvent()
            {
                EventStart = DateTime.UtcNow.AddSeconds(65),
                TotalTimeMs = 500
            });
            workTime.PendingEvents.Count.Should().Be(0);

            var found = _repository.Find(workTime.User, DateTime.UtcNow);
            found.KeyboardActionEvents.Count.Should().Be(1);
            found.MouseActionEvents.Count.Should().Be(1);
        }

        [Fact]
        public void g()
        {
            var workTime =
                _buildService.CreateStartedManually(UserTestUtils.CreateTestUser(), DateTime.UtcNow.AddMinutes(60));
            _service.SetWorkTime(workTime);
            _service.KeyboardEventBufferSz = 1;
            _service.MouseEventBufferSz = 1;
            workTime.StartManually();

            _service.StartTempChanges();

            _service.AddKeyboardEvent(new MonitorEvent()
            {
                EventStart = DateTime.UtcNow,
                TotalTimeMs = 200,
            });

            _service.AddMouseEvent(new MonitorEvent()
            {
                EventStart = DateTime.UtcNow,
                TotalTimeMs = 500
            });

            _service.AddKeyboardEvent(new MonitorEvent()
            {
                EventStart = DateTime.UtcNow.AddSeconds(65),
                TotalTimeMs = 200,
            });

            _service.AddMouseEvent(new MonitorEvent()
            {
                EventStart = DateTime.UtcNow.AddSeconds(65),
                TotalTimeMs = 500
            });

            var found = _repository.Find(workTime.User, DateTime.UtcNow);
            found.KeyboardActionEvents.Count.Should().Be(0);
            found.MouseActionEvents.Count.Should().Be(0);


            _service.CommitTempChanges();
            workTime.PendingEvents.Count.Should().Be(0);


            found = _repository.Find(workTime.User, DateTime.UtcNow);
            found.KeyboardActionEvents.Count.Should().Be(1);
            found.MouseActionEvents.Count.Should().Be(1);
        }
    }
}