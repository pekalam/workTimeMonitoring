using System;
using AutoMapper;
using Domain;
using Domain.Repositories;
using Infrastructure.Db;
using Infrastructure.Repositories;
using Xunit;

namespace Infrastructure.Tests
{
    [Trait("Category", "Integration")]
    public class SqliteWorkTimeEsRepositoryTests : WorkTimeEsRepositoryTests, IDisposable
    {
        protected override IWorkTimeEsRepository CreateRepository()
        {
            var mapper = new MapperConfiguration(opt =>
            {
                opt.AddProfile<DbEventProfile>();
            });
            return new SqliteWorkTimeEsRepository(new SqliteSettings(), mapper.CreateMapper());
        }

        public void Dispose()
        {
            SqliteTestUtils.TruncTable(SqliteWorkTimeEsRepository.TableName);
        }
    }
}