using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;
using AutoMapper;
using Dapper;
using Domain;
using Domain.Repositories;
using Domain.Services;
using Domain.User;
using Domain.WorkTimeAggregate;
using Domain.WorkTimeAggregate.Events;
using Infrastructure.Db;
using Newtonsoft.Json;

namespace Infrastructure.Repositories
{
    
    public class SqliteWorkTimeEsRepository : IWorkTimeEsRepository
    {
        public const string TableName = "WorkTimeEvent";
        private const string TableCols = "Id, EventName, AggregateId, AggregateVersion, Date, Data";

        private readonly SqliteSettings _sqliteSettings;
        private readonly IMapper _mapper;

        static SqliteWorkTimeEsRepository()
        {
            SqlMapper.RemoveTypeMap(typeof(DateTime));
            SqlMapper.AddTypeHandler(new DateTimeHandler());
        }

        public SqliteWorkTimeEsRepository(IConfigurationService configurationService, IMapper mapper)
        {
            _sqliteSettings = configurationService.Get<SqliteSettings>("sqlite");
            _mapper = mapper;
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
            var sql = $@"SELECT COUNT(*) FROM {TableName} WHERE EventName = @EventName AND json_extract(Data, '$.User.Username.Value') = @Username";

            using var conn = CreateConnection(true);
            var count = conn.ExecuteScalar<int>(sql, new {EventName=(int)EventName.WorkTimeCreated, Username=user.Username.Value});
            return count;
        }

        public void Save(WorkTime workTime)
        {
            var sql = $@"INSERT INTO {TableName} ({TableCols}) VALUES (NULL, @EventName, @AggregateId, @AggregateVersion, @Date, @Data)";
            using var conn = CreateConnection(false);
            using var trans = conn.BeginTransaction();

            foreach (var ev in workTime.PendingEvents)
            {
                var dbEvent = _mapper.Map<DbEvent>(ev);
                conn.Execute(sql, dbEvent, trans);
            }

            trans.Commit();
        }


        //todo Username = user?
        List<WorkTime> IWorkTimeEsRepository.FindAll(User user, DateTime startDate, DateTime endDate)
        {
            throw new NotImplementedException();
        }

        public WorkTime? Find(User user, DateTime date)
        {
            //todo json_extract(Data, '$.DateCreated') <=> Date - error
            var sql = $@"SELECT {TableCols} FROM {TableName}
                               WHERE AggregateId = (SELECT AggregateId FROM {TableName}
                                                    WHERE EventName = @EventName AND json_extract(Data, '$.User.Username.Value') = @Username
                                                                           AND json_extract(Data, '$.DateCreated') <= @Date
                                                                           AND json_extract(Data, '$.EndDate') >= @Date 
                                                                           ORDER BY Date DESC LIMIT 1);";
                                
            using var conn = CreateConnection(true);

            var events = conn.Query<DbEvent>(sql, new
                {
                    Username = user.Username.Value,
                    Date = date,
                    EventName=EventName.WorkTimeCreated,
                })
                .MapToEvents(_mapper);

            var workTime = WorkTime.FromEvents(events);

            return workTime;
        }

        public WorkTime? FindFromSnapshot(WorkTimeSnapshotCreated snapshotEvent)
        {
            var sql = $@"SELECT {TableCols} FROM {TableName} WHERE AggregateVersion = @AggregateVersion";
            using var conn = CreateConnection(true);

            var events = conn.Query<DbEvent>(sql, new
                {
                    AggregateVersion = snapshotEvent.AggregateVersion,
                })
                .MapToEvents(_mapper).ToList();

            if (events.Count != 1)
            {
                throw new Exception();
            }

            var snap = events.First() as WorkTimeSnapshotCreated;

            if (snap == null)
            {
                throw new Exception();
            }

            var workTime = WorkTime.CreateFromSnapshot(snap);

            return workTime;
        }

        public void Rollback(WorkTimeSnapshotCreated snapshotEvent)
        {
            var sql = $@"DELETE FROM {TableName} WHERE AggregateId = @AggregateId AND AggregateVersion > @AggregateVersion";

            using var conn = CreateConnection(false);
            using var trans = conn.BeginTransaction();

            var result = conn.Execute(sql, new {AggregateId=snapshotEvent.AggregateId, AggregateVersion=snapshotEvent.AggregateVersion});

            if (result == 0)
            {
                throw new Exception($"Cannot find {snapshotEvent.AggregateId} aggregate with event ver gt {snapshotEvent.AggregateVersion}");
            }

            trans.Commit();
        }
    }
}
