using System;
using Infrastructure.Domain;

namespace Infrastructure.Repositories
{
    public interface IWorkTimeEsRepository
    {
        public int CountForUser(User user);
        public void Save(WorkTime workTime);
        public WorkTime Rollback(WorkTimeSnapshotCreated snapshotEvent);
        public WorkTime? Find(User user, DateTime startDate, DateTime endDate);
        public WorkTime? FindFromLastSnapshot(User user);
    }
}
