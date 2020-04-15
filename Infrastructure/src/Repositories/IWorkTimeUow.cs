using Infrastructure.Domain;

namespace Infrastructure.Repositories
{
    public interface IWorkTimeUow
    {
        void Register(WorkTime workTime);
        void Rollback();
        void Save();
        void Commit();
    }
}
