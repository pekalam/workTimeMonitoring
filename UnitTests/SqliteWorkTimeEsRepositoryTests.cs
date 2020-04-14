using System;
using System.Data.SQLite;
using Dapper;
using Infrastructure.Db;
using Infrastructure.Repositories;
using Infrastructure.src.Repositories;

namespace UnitTests
{
    public class SqliteWorkTimeEsRepositoryTests : WorkTimeEsRepositoryTests, IDisposable
    {
        protected override IWorkTimeEsRepository CreateRepository()
        {
            return new SqliteWorkTimeEsRepository(new SqliteSettings());
        }

        public void Dispose()
        {
            using var conn = new SQLiteConnection(new SqliteSettings().ConnectionString);
            conn.Execute($"DELETE FROM {SqliteWorkTimeEsRepository.TableName};");
        }
    }
}