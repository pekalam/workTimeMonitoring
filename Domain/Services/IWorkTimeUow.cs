namespace Domain.Services
{
    public interface IWorkTimeUow
    {
        void RegisterNew(WorkTime.WorkTime workTime);
        void Rollback();
        void Save();
        void Commit();
    }
}
