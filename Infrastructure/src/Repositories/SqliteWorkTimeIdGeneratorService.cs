using Dapper;
using Domain.Services;
using Infrastructure.Db;
using System.Data.SQLite;

namespace Infrastructure.Repositories
{
    internal class SqliteWorkTimeIdGeneratorService : IWorkTimeIdGeneratorService
    {
        public const string IdSequence = "WorkTimeIdSequence";

        private readonly SqliteSettings _sqliteSettings;

        public SqliteWorkTimeIdGeneratorService(IConfigurationService configurationService)
        {
            _sqliteSettings = configurationService.Get<SqliteSettings>("sqlite");
        }
        
        public long GenerateId()
        {
            var sql = $"INSERT INTO {IdSequence} VALUES(NULL); SELECT last_insert_rowid();";

            using var c = new SQLiteConnection(_sqliteSettings.ConnectionString);
            c.Open();
            using var trans = c.BeginTransaction();

            var id = c.ExecuteScalar<long>(sql);

            trans.Commit();

            return id;
        }
    }
}