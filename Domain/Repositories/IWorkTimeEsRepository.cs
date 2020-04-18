using System;
using System.Collections.Generic;
using Domain.WorkTimeAggregate.Events;

namespace Domain.Repositories
{
    public interface IWorkTimeEsRepository
    {
        public int CountForUser(User.User user);
        public void Save(WorkTimeAggregate.WorkTime workTime);
        public List<WorkTimeAggregate.WorkTime> FindAll(User.User user, DateTime startDate, DateTime endDate);
        public WorkTimeAggregate.WorkTime? Find(User.User user, DateTime date);
        public WorkTimeAggregate.WorkTime? FindFromSnapshot(WorkTimeSnapshotCreated snapshotEvent);
        public void Rollback(WorkTimeSnapshotCreated snapshot);
    }
}