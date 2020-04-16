using System;
using System.Data.SQLite;
using AutoMapper;
using Dapper;
using Infrastructure.Db;
using Infrastructure.Repositories;

namespace UnitTests
{
    public static class SqliteTestUtils
    {
        public static void TruncTable(string tableName)
        {
            using var conn = new SQLiteConnection(new SqliteSettings().ConnectionString);
            conn.Execute($"DELETE FROM {tableName};");
        }
    }

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