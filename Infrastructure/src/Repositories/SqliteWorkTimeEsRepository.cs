using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;
using System.Text;
using Dapper;
using Infrastructure.Db;
using Infrastructure.Domain;
using Infrastructure.Repositories;
using Newtonsoft.Json;

namespace Infrastructure.src.Repositories
{
    public class SqliteWorkTimeEsRepository : IWorkTimeEsRepository
    {
        public const string TableName = "WorkTimeEvent";

        private readonly SqliteSettings _sqliteSettings;

        public SqliteWorkTimeEsRepository(SqliteSettings sqliteSettings)
        {
            _sqliteSettings = sqliteSettings;
        }

        private SQLiteConnection CreateConnection(bool loadExt = true)
        {
            var c = new SQLiteConnection(_sqliteSettings.ConnectionString);
            c.Open();
            if (loadExt)
            {
                c.EnableExtensions(true);
                c.LoadExtension("json1.dll");
            }
            return c;
        }

        public int CountForUser(User user)
        {
            var sql = $@"SELECT COUNT(*) FROM {TableName} WHERE";
            return 0;
        }

        public void Save(WorkTime workTime)
        {
            var sql = $@"INSERT INTO {TableName} (Id,AggregateId,EventName,Date,Data,AggregateVersion) VALUES ( NULL, @AggregateId, @EventName, @Date, @Data, @AggregateVersion )";
            using var conn = CreateConnection(false);
            var trans = conn.BeginTransaction();

            foreach (var ev in workTime.PendingEvents)
            {
                var Data = JsonConvert.SerializeObject(ev, settings: new JsonSerializerSettings()
                {
                    TypeNameHandling = TypeNameHandling.All,
                    DateFormatString = "yyyy'-'MM'-'dd' 'HH':'mm':'ss.FFFFFFFK",
                });
                conn.Execute(sql, new
                {
                    ev.AggregateId, EventName="ev1", Date = ev.Date, Data, AggregateVersion=0
                }, trans);
            }

            trans.Commit();
        }

        public WorkTime Rollback(WorkTimeSnapshotCreated snapshotEvent)
        {
            var sql = $@"DELETE * FROM {TableName} WHERE AggregateVersion > @SnapshotAggVersion";

            using var conn = CreateConnection(false);


            return null;
        }

        public WorkTime? Find(User user, DateTime startDate, DateTime endDate)
        {
            var sql = $@"SELECT Data FROM {TableName} WHERE json_extract(Data, @Q1) = @Username AND json_extract(Data, @Q2) >= @Val";
            using var conn = CreateConnection(true);

            var events = conn.Query<string>(sql, new
            {
                Q1 = "$.User.Username.Value",
                Username = user.Username.Value,
                Q2 = "$.Date", 
                Val= startDate, 
            })
                .Select(json =>
                {
                    var ev = JsonConvert.DeserializeObject(json, new JsonSerializerSettings()
                    {
                        TypeNameHandling = TypeNameHandling.All
                    });
                    if (ev == null)
                    {
                        throw new Exception("Deserialization error: null event");
                    }

                    return ev as Event;
                }).ToList();

            var workTime = WorkTime.FromEvents(events);

            return workTime;
        }

        public WorkTime? FindFromLastSnapshot(User user)
        {
            return null;
        }
    }
}
