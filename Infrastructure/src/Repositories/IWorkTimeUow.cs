using Infrastructure.Domain;

namespace Infrastructure.Repositories
{
    public interface IWorkTimeUow
    {
        void RegisterNew(WorkTime workTime);
        void Rollback();
        void Save();
        void Commit();
    }
}
