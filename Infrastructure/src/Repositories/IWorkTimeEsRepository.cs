using System;
using Infrastructure.Domain;

namespace Infrastructure.Repositories
{
    public interface IWorkTimeEsRepository
    {
        public void Save(WorkTime workTime);
        public void Rollback(WorkTimeSnapshotCreated snapshotEvent);
        public WorkTime? Find(DateTime startDate, DateTime endDate);
        public WorkTime? FindFromSnapshot(DateTime? startDate);
    }
}
