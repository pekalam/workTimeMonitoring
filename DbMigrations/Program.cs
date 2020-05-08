using Serilog;
using System;
using System.Data.SQLite;
using System.IO;

namespace DbMigrations
{
    class Program
    {
        static void Main(string[] args)
        {
            const string dbName = "appdb.db";

            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .WriteTo.Console()
                .CreateLogger();

            SQLiteConnection.CreateFile(dbName);

            try
            {
                var cnx = new SQLiteConnection(@$"Data Source=.\{dbName};");
                var evolve = new Evolve.Evolve(cnx, msg => Log.Logger.Information(msg))
                {
                    Locations = new[] { "migrations" },
                    IsEraseDisabled = true,
                };

                evolve.Migrate();

                File.Copy(dbName, $"../../../../Infrastructure/{dbName}", true);
                File.Copy(dbName, $"../../../../Infrastructure.Tests/test{dbName}", true);
            }
            catch (Exception ex)
            {
                Console.WriteLine();
                Log.Logger.Fatal("Database migration failed.", ex);
                throw;
            }
        }
    }
}
