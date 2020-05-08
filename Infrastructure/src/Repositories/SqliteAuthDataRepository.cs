using Dapper;
using Domain.Repositories;
using Domain.Services;
using Domain.User;
using Infrastructure.Db;
using System;
using System.Data.SQLite;
using System.Linq;

namespace Infrastructure.src.Repositories
{
    internal class SqliteAuthDataRepository : IAuthDataRepository
    {
        public const string TableName = "AuthData";
        private const string Columns = "UserId, Password";

        private SqliteSettings _settings;

        public SqliteAuthDataRepository(IConfigurationService configurationService)
        {
            _settings = configurationService.Get<SqliteSettings>("sqlite");
        }

        private SQLiteConnection CreateConnection()
        {
            var conn = new SQLiteConnection(_settings.ConnectionString);
            conn.Open();
            return conn;
        }


        public AuthData Find(long userId)
        {
            var sql = $@"SELECT {Columns} FROM {TableName} WHERE UserId = @UserId";
            using var conn = CreateConnection();

            var authData = conn.Query<AuthData>(sql, new {UserId = userId}).FirstOrDefault();

            if (authData == null)
            {
                throw new Exception($"AuthData not found for user {userId}");
            }

            return authData;
        }

        public void Add(AuthData authData)
        {
            throw new NotImplementedException();
        }
    }
}
