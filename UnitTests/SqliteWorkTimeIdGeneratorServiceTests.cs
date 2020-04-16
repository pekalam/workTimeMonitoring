using System;
using Infrastructure.Db;
using Infrastructure.Repositories;

namespace UnitTests
{
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