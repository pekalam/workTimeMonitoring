using System;
using System.Collections.Generic;
using Domain.WorkTimeAggregate;
using Domain.WorkTimeAggregate.Events;

namespace Domain.Repositories
{
    public interface IWorkTimeEsRepository
    {
        public int CountForUser(User.User user);
        public void Save(WorkTime workTime);
        public List<WorkTime> FindAll(User.User user, DateTime startDate, DateTime endDate);
        public WorkTime? Find(User.User user, DateTime date);
        public WorkTime? FindFromSnapshot(WorkTimeSnapshotCreated snapshotEvent);
        public WorkTime? FindLatestFromSnapshot(User.User user);
        public void Rollback(WorkTimeSnapshotCreated snapshot);
    }
}