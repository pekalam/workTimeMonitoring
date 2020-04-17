using System;
using System.Collections.Generic;
using Domain.WorkTime.Events;

namespace Domain.Repositories
{
    public interface IWorkTimeEsRepository
    {
        public int CountForUser(User.User user);
        public void Save(WorkTime.WorkTime workTime);
        public List<WorkTime.WorkTime> FindAll(User.User user, DateTime startDate, DateTime endDate);
        public WorkTime.WorkTime? Find(User.User user, DateTime date);
        public WorkTime.WorkTime? FindFromSnapshot(WorkTimeSnapshotCreated snapshotEvent);
        public void Rollback(WorkTimeSnapshotCreated snapshot);
    }
}