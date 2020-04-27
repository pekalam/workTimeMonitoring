using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using Dapper;
using Domain.Repositories;
using Domain.Services;
using Domain.User;
using Infrastructure.Db;

namespace Infrastructure.src.Repositories
{
    internal class SqliteUserRepository : IUserRepository
    {
        public const string TableName = "User";
        private const string Columns = "UserId, Username";
        private readonly SqliteSettings _settings;

        public SqliteUserRepository(IConfigurationService configurationService)
        {
            _settings = configurationService.Get<SqliteSettings>("sqlite");
        }

        private SQLiteConnection CreateConnection()
        {
            var conn = new SQLiteConnection(_settings.ConnectionString);
            conn.Open();
            return conn;
        }


        public User? Find(Username username)
        {
            var sql = $@"SELECT {Columns} FROM {TableName} WHERE Username = @Username";
            using var conn = CreateConnection();

            var found = conn.Query<User>(sql, new {Username = username.Value}).FirstOrDefault();

            return found;
        }
    }
}
