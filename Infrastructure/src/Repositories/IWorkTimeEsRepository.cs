using System;
using System.Collections.Generic;
using Infrastructure.Domain;

namespace Infrastructure.Repositories
{
    public interface IWorkTimeEsRepository
    {
        public int CountForUser(User user);
        public void Save(WorkTime workTime);
        public List<WorkTime> FindAll(User user, DateTime startDate, DateTime endDate);
        public WorkTime? Find(User user, DateTime date);
        public WorkTime? FindFromSnapshot(WorkTimeSnapshotCreated snapshotEvent);
        public void Rollback(WorkTimeSnapshotCreated snapshot);
    }

    public interface IWorkTimeIdGeneratorService
    {
        public long GenerateId();
    }
}
