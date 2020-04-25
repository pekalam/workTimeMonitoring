using System;
using AutoMapper;
using Domain;
using Domain.Repositories;
using Domain.Services;
using Domain.User;
using Domain.WorkTimeAggregate;
using DomainTestUtils;
using FluentAssertions;
using Infrastructure.Db;
using Infrastructure.Repositories;
using Infrastructure.Services;
using Infrastructure.src.Services;
using Xunit;

namespace Infrastructure.Tests
{
    public abstract class WorkTimeIdGeneratorServiceTests
    {
        protected IWorkTimeIdGeneratorService _generatorService;

        public WorkTimeIdGeneratorServiceTests()
        {
            _generatorService = Create();
        }

        protected abstract IWorkTimeIdGeneratorService Create();

        [Fact]
        public void GenerateId_generates_unique_greater_than0_ids()
        {
            var id1 = _generatorService.GenerateId();

            id1.Should().BeGreaterThan(0);


            var id2 = _generatorService.GenerateId();


            id2.Should().BeGreaterThan(0);
            id1.Should().NotBe(id2, "Should be unique");
        }
    }

    public abstract class WorkTimeUowTests
    {
        private IWorkTimeUow _uow;
        private IWorkTimeEsRepository _repository;

        public WorkTimeUowTests()
        {
            _repository = CreateRepo();
            _uow = Create(_repository);
        }

        protected abstract IWorkTimeUow Create(IWorkTimeEsRepository repository);

        protected abstract IWorkTimeEsRepository CreateRepo();

        [Fact]
        public void f()
        {
            var workTime = WorkTimeTestUtils.CreateManual();
            _repository.Save(workTime);
            workTime.MarkPendingEventsAsHandled();

            for (int i = 0; i < 2; i++)
            {
                _uow.RegisterNew(workTime);

                workTime.StartManually();

                _uow.Save();

                var found = _repository.Find(workTime.User, DateTime.UtcNow);

                found.Started.Should().BeTrue();

                _uow.Rollback();

                found = _repository.Find(workTime.User, DateTime.UtcNow);

                found.Started.Should().BeFalse();
                _repository.CountForUser(workTime.User).Should().Be(1);
            }

        }
    }

    public class SqliteWorkTimeUowTests : WorkTimeUowTests, IDisposable
    {
        public SqliteWorkTimeUowTests() : base()
        {
        }

        protected override IWorkTimeUow Create(IWorkTimeEsRepository repository)
        {
            return new WorkTimeUow(repository);
        }

        protected override IWorkTimeEsRepository CreateRepo()
        {
            return new SqliteWorkTimeEsRepository(TestUtils.ConfigurationService,
                new MapperConfiguration(opt => opt.AddProfile<DbEventProfile>()).CreateMapper());
        }

        public void Dispose()
        {
            SqliteTestUtils.TruncTable(SqliteWorkTimeEsRepository.TableName);
        }
    }

    public abstract class WorkTimeBuildServiceTests
    {
        private readonly IWorkTimeIdGeneratorService _idGenerator;
        private readonly IWorkTimeEsRepository _repository;
        protected WorkTimeBuildService _service;

        public WorkTimeBuildServiceTests()
        {
            _idGenerator = CreateGenService();
            _repository = CreateRepo();
            _service = new WorkTimeBuildService(_repository, _idGenerator);
        }

        protected abstract IWorkTimeIdGeneratorService CreateGenService();
        protected abstract IWorkTimeEsRepository CreateRepo();

        [Fact]
        public void Create()
        {
            var workTime = _service.CreateStartedManually(new User(1, "mpekala"), DateTime.UtcNow.AddMinutes(60));
            workTime.AggregateId.Should().BeGreaterThan(-1);
            workTime.PendingEvents.Count.Should().Be(0);
        }
    }

    public class SqliteWorkTimeBuildServiceTests : WorkTimeBuildServiceTests, IDisposable
    {
        protected override IWorkTimeIdGeneratorService CreateGenService()
        {
            return new SqliteWorkTimeIdGeneratorService(TestUtils.ConfigurationService);
        }

        protected override IWorkTimeEsRepository CreateRepo()
        {
            return new SqliteWorkTimeEsRepository(TestUtils.ConfigurationService,
                new MapperConfiguration(opt => opt.AddProfile<DbEventProfile>()).CreateMapper());
        }

        public void Dispose()
        {
            SqliteTestUtils.TruncTable(SqliteWorkTimeEsRepository.TableName);
        }
    }
}