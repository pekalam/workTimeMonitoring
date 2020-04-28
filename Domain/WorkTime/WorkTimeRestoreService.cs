using System;
using System.Collections.Generic;
using System.Text;
using Domain.Repositories;

namespace Domain.WorkTimeAggregate
{
    public class WorkTimeRestoreService
    {
        private readonly IWorkTimeEsRepository _repository;

        public WorkTimeRestoreService(IWorkTimeEsRepository repository)
        {
            _repository = repository;
        }

        public void SetInterrupted(WorkTime workTime)
        {
            workTime.TakeSnapshot();
            workTime.SetInterrupted();
            _repository.Save(workTime);
            workTime.MarkPendingEventsAsHandled();
        }

        public bool Restore(User.User user, out WorkTime? restored)
        {
            var restoredWorkTime = _repository.FindLatestFromSnapshot(user);
            if (restoredWorkTime != null)
            {
                if (restoredWorkTime.Stopped)
                {
                    restored = null;
                    return false;
                }

                restoredWorkTime.SetRestored();
                _repository.Save(restoredWorkTime);
                restoredWorkTime.MarkPendingEventsAsHandled();

                restored = restoredWorkTime;
                return true;
            }

            restored = null;
            return false;
        }
    }
}
