using Domain.WorkTimeAggregate;

namespace Domain.Services
{
    public interface IWorkTimeUow
    {
        void RegisterNew(WorkTime workTime);
        void Unregister(WorkTime workTime);
        void Rollback();
        void Save();
        void Commit();
        bool HasRegistered { get; }
    }
}
