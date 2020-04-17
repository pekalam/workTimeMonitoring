using System.Data.SQLite;
using Dapper;
using Infrastructure.Db;

namespace Infrastructure.Tests
{
    public static class SqliteTestUtils
    {
        public static void TruncTable(string tableName)
        {
            using var conn = new SQLiteConnection(new SqliteSettings().ConnectionString);
            conn.Execute($"DELETE FROM {tableName};");
        }
    }
}