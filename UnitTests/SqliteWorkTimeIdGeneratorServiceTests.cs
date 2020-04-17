using System;
using Domain;
using Domain.Services;
using Infrastructure.Db;
using Infrastructure.Repositories;
using Xunit;

namespace Infrastructure.Tests
{
    [Trait("Category", "Integration")]
    public class SqliteWorkTimeIdGeneratorServiceTests : WorkTimeIdGeneratorServiceTests, IDisposable
    {
        protected override IWorkTimeIdGeneratorService Create()
        {
            return new SqliteWorkTimeIdGeneratorService(new SqliteSettings());
        }

        public void Dispose()
        {
            SqliteTestUtils.TruncTable(SqliteWorkTimeIdGeneratorService.IdSequence);
        }
    }
}