using System.Data.SQLite;
using Dapper;
using Domain;
using Domain.Services;
using Infrastructure.Db;

namespace Infrastructure.Repositories
{
    public class SqliteWorkTimeIdGeneratorService : IWorkTimeIdGeneratorService
    {
        public const string IdSequence = "WorkTimeIdSequence";

        private readonly SqliteSettings _sqliteSettings;

        public SqliteWorkTimeIdGeneratorService(SqliteSettings sqliteSettings)
        {
            _sqliteSettings = sqliteSettings;
        }
        
        public long GenerateId()
        {
            var sql = $"INSERT INTO {IdSequence} VALUES(NULL); SELECT last_insert_rowid();";

            using var c = new SQLiteConnection(_sqliteSettings.ConnectionString);
            c.Open();
            var trans = c.BeginTransaction();

            var id = c.ExecuteScalar<long>(sql);

            trans.Commit();

            return id;
        }
    }
}