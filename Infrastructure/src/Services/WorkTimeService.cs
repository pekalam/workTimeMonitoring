using System;
using Infrastructure.Domain;
using Infrastructure.Repositories;

namespace Infrastructure.Services
{
    public class WorkTimeService
    {
        private WorkTimeSnapshotCreated? _snapshotCreatedEvent;
        private Domain.WorkTime _workTime = new Domain.WorkTime();
        private IWorkTimeEsRepository _esRepository;

        public void AddMouseAction()
        {
            _workTime.AddMouseAction();
            _esRepository.Save(_workTime);
        }

        public void AddKeyboardAction()
        {
            _workTime.AddKeyboardAction();
        }

        private void SaveIfEventCountExceded()
        {
            if (_workTime.ActionEvents.Count > 200)
            {
                _esRepository.Save(_workTime);
                _workTime.MarkPendingEventsAsHandled();
            }
        }

        public void DiscardTemporaryChanges()
        {
            if (_snapshotCreatedEvent == null)
            {
                throw new Exception("StartTemporaryChanges not called");
            }

            _workTime.RollbackToSnapshot(_snapshotCreatedEvent);
            _esRepository.Rollback(_snapshotCreatedEvent);
        }

        public void StartTemporaryChanges()
        {
            _snapshotCreatedEvent = _workTime.TakeSnapshot();
            _esRepository.Save(_workTime);
            _workTime.MarkPendingEventsAsHandled();
        }

        public void EndTemporaryChanges()
        {
            _snapshotCreatedEvent = null;
        }

    }
}
