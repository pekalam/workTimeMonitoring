using System;
using System.Diagnostics;
using System.Threading;
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
        private DateTime _start;

        protected override IWorkTimeEsRepository CreateRepository()
        {
            _start = DateTime.UtcNow;
            
            var mapper = new MapperConfiguration(opt =>
            {
                opt.AddProfile<DbEventProfile>();
            });
            return new SqliteWorkTimeEsRepository(TestUtils.ConfigurationService, mapper.CreateMapper());
        }

        public void Dispose()
        {
            Debug.WriteLine(_start);
            SqliteTestUtils.TruncTable(SqliteWorkTimeEsRepository.TableName);
        }
    }
}