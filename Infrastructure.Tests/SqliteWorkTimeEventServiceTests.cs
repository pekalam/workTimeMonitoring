using System;
using AutoMapper;
using Domain.Repositories;
using Domain.Services;
using Infrastructure.Db;
using Infrastructure.Repositories;
using Infrastructure.src.Services;

namespace Infrastructure.Tests
{
    public class SqliteWorkTimeEventServiceTests : WorkTimeEventServiceTests, IDisposable
    {
        protected override IWorkTimeUow CreateUow()
        {
            return new WorkTimeUow(CreateRepo());
        }

        protected override IWorkTimeEsRepository CreateRepo()
        {
            var mapper = new MapperConfiguration(opt =>
            {
                opt.AddProfile<DbEventProfile>();
            });
            return new SqliteWorkTimeEsRepository(TestUtils.ConfigurationService, mapper.CreateMapper());
        }

        protected override IWorkTimeIdGeneratorService CreateIdGen()
        {
            return new SqliteWorkTimeIdGeneratorService(TestUtils.ConfigurationService);
        }

        public void Dispose()
        {
            SqliteTestUtils.TruncTable(SqliteWorkTimeEsRepository.TableName);
            SqliteTestUtils.TruncTable(SqliteWorkTimeIdGeneratorService.IdSequence);
        }
    }
}