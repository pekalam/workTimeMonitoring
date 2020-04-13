namespace Infrastructure.Db
{
    public class SqliteSettings
    {
        public string ConnectionString { get; set; } = @"Data Source=.\appdb.db;";
    }
}