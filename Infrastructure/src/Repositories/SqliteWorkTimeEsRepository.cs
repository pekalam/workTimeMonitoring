using AutoMapper;
using Dapper;
using Domain.Repositories;
using Domain.Services;
using Domain.User;
using Domain.WorkTimeAggregate;
using Domain.WorkTimeAggregate.Events;
using Infrastructure.Db;
using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;

namespace Infrastructure.Repositories
{

    internal class SqliteWorkTimeEsRepository : IWorkTimeEsRepository
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
            var sql = $@"SELECT COUNT(*) FROM {TableName} WHERE EventName = @EventName AND json_extract(Data, '$.User.UserId') = @UserId";

            using var conn = CreateConnection(true);
            var count = conn.ExecuteScalar<int>(sql, new {EventName=(int)EventName.WorkTimeCreated, UserId=user.UserId});
            return count;
        }

        public void Save(WorkTime workTime)
        {
            var sql = $@"INSERT INTO {TableName} ({TableCols}) VALUES (NULL, @EventName, @AggregateId, @AggregateVersion, @Date, @Data)";
            using var conn = CreateConnection(false);
            using var trans = conn.BeginTransaction();

            foreach (var ev in workTime.PendingEvents.ToList())
            {
                var dbEvent = _mapper.Map<DbEvent>(ev);
                conn.Execute(sql, dbEvent, trans);
            }

            trans.Commit();
        }

        private IEnumerable<IGrouping<long, Event>> AllForUser(User user, SQLiteConnection conn)
        {
            var sql = $@"SELECT {TableCols} FROM {TableName}
                               WHERE AggregateId IN (SELECT AggregateId FROM {TableName}
                                                    WHERE EventName = @EventName AND json_extract(Data, '$.User.UserId') = @UserId)
                               ORDER BY AggregateId, AggregateVersion";

            return conn.Query<DbEvent>(sql, new
                {
                    UserId = user.UserId,
                    EventName = EventName.WorkTimeCreated,
                })
                .MapToEvents(_mapper)
                .GroupBy(e => e.AggregateId);
        }

        private IEnumerable<IGrouping<long, Event>> AllForUserInRange(User user, DateTime startDate, DateTime endDate,
            SQLiteConnection conn)
        {
            var sql = $@"SELECT {TableCols} FROM {TableName}
                               WHERE AggregateId IN (SELECT AggregateId FROM {TableName}
                                                    WHERE EventName = @EventName AND json_extract(Data, '$.User.UserId') = @UserId
                                                    AND json_extract(Data, '$.DateCreated') >= @StartDate
                                                    AND json_extract(Data, '$.EndDate') <= @EndDate) 
                               ORDER BY AggregateId, AggregateVersion";

            return conn.Query<DbEvent>(sql, new
                {
                    UserId = user.UserId,
                    EventName = EventName.WorkTimeCreated,
                    StartDate = startDate,
                    EndDate = endDate,
                })
                .MapToEvents(_mapper)
                .GroupBy(e => e.AggregateId);
        }

        public List<WorkTime> FindAll(User user, DateTime? startDate, DateTime? endDate)
        {
            using var conn = CreateConnection(true);

            IEnumerable<IGrouping<long, Event>>? events = null;
            if (startDate == null && endDate == null)
            {
                events = AllForUser(user, conn);
            }else if (startDate != null && endDate != null)
            {
                events = AllForUserInRange(user, startDate.Value, endDate.Value, conn);
            }

            if (events == null)
            {
                throw new ArgumentException();
            }

            var workTimes = new List<WorkTime>();
            
            foreach (var aggEvents in events)
            {
                workTimes.Add(WorkTime.FromEvents(aggEvents));   
            }

            return workTimes;
        }

        public WorkTime? Find(User user, DateTime date)
        {
            //todo json_extract(Data, '$.DateCreated') <=> Date - error
            var sql = $@"SELECT {TableCols} FROM {TableName}
                               WHERE AggregateId = (SELECT AggregateId FROM {TableName}
                                                    WHERE EventName = @EventName AND json_extract(Data, '$.User.UserId') = @UserId
                                                                           AND json_extract(Data, '$.DateCreated') <= @Date
                                                                           AND json_extract(Data, '$.EndDate') >= @Date 
                                                                           ORDER BY Date DESC LIMIT 1);";
                                
            using var conn = CreateConnection(true);

            var events = conn.Query<DbEvent>(sql, new
                {
                    UserId = user.UserId,
                    Date = date,
                    EventName=EventName.WorkTimeCreated,
                })
                .MapToEvents(_mapper).ToList();

            if (events.Count == 0)
            {
                return null;
            }

            var workTime = WorkTime.FromEvents(events);

            return workTime;
        }

        public WorkTime? FindFromSnapshot(WorkTimeSnapshotCreated snapshotEvent)
        {
            var sql = $@"SELECT {TableCols} FROM {TableName} WHERE AggregateId = @AggregateId AND AggregateVersion = @AggregateVersion";
            using var conn = CreateConnection(true);

            var events = conn.Query<DbEvent>(sql, new
                {
                    AggregateVersion = snapshotEvent.AggregateVersion,
                    AggregateId = snapshotEvent.AggregateId,
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

        public WorkTime? FindLatestFromSnapshot(User user)
        {
            var sql = $@"SELECT {TableCols} FROM {TableName}
                               INNER JOIN (SELECT AggregateId as 'idsnap', AggregateVersion as 'ver' 
                                            FROM {TableName} WHERE EventName=@EventName
                                            AND json_extract(Data, '$.Snapshot.User.UserId') = @UserId 
                                            ORDER BY Date DESC LIMIT 1) snap
                               ON AggregateId = snap.idsnap
                               AND AggregateVersion >= snap.ver";
            using var conn = CreateConnection(true);

            var events = conn.Query<DbEvent>(sql, new
                {
                    EventName = EventName.WorkTimeSnapshotCreated,
                    UserId=user.UserId,

                })
                .MapToEvents(_mapper).ToList();

            if (events.Count == 0)
            {
                return null;
            }

            var workTime = WorkTime.CreateFromSnapshot(events);

            return workTime;
        }

        public void Rollback(WorkTimeSnapshotCreated snapshotEvent)
        {
            var sql = $@"DELETE FROM {TableName} WHERE AggregateId = @AggregateId AND AggregateVersion > @AggregateVersion";

            using var conn = CreateConnection(false);
            using var trans = conn.BeginTransaction();

            var result = conn.Execute(sql, new {snapshotEvent.AggregateId, snapshotEvent.AggregateVersion});

            trans.Commit();
        }
    }
}
