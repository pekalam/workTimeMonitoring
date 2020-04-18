using System;
using System.Collections.Generic;
using System.Text;
using Domain.Repositories;
using Domain.Services;
using Domain.WorkTimeAggregate;
using Domain.WorkTimeAggregate.Events;

namespace Infrastructure.src.Services
{
    public class WorkTimeUow : IWorkTimeUow
    {
        private readonly IWorkTimeEsRepository _repository;
        private WorkTimeSnapshotCreated? _snapshot;
        private WorkTime? _workTime;

        public WorkTimeUow(IWorkTimeEsRepository repository)
        {
            _repository = repository;
        }

        public void RegisterNew(WorkTime workTime)
        {
            _workTime = workTime;
            _snapshot = _workTime.TakeSnapshot();
            _repository.Save(workTime);
            _workTime.MarkPendingEventsAsHandled();
        }

        public void Unregister(WorkTime workTime)
        {
            if (workTime == _workTime)
            {
                _workTime = null;
                _snapshot = null;
            }
        }

        public void Rollback()
        {
            _repository.Rollback(_snapshot);
            _workTime.RollbackToSnapshot(_snapshot);
        }

        public void Save()
        {
            _repository.Save(_workTime);
            _workTime.MarkPendingEventsAsHandled();
        }

        public void Commit()
        {
            _repository.Save(_workTime);
            _workTime.MarkPendingEventsAsHandled();
            _snapshot = null;
        }

        public bool HasRegistered => _workTime != null;
    }
}
