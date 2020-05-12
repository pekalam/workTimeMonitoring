using System;
using System.Collections.Generic;
using Domain.Repositories;
using Domain.User;
using Domain.WorkTimeAggregate;
using Domain.WorkTimeAggregate.Events;
using WMAlghorithm.Services;

namespace WindowUI
{
    internal class WorkTimeEsRepositorDecorator : IWorkTimeEsRepository
    {
        private readonly IWorkTimeEsRepository _repository;
        private readonly AlgorithmService _moduleService;

        public WorkTimeEsRepositorDecorator(IWorkTimeEsRepository repository, AlgorithmService moduleService)
        {
            _repository = repository;
            _moduleService = moduleService;
        }

        public int CountForUser(User user)
        {
            return _repository.CountForUser(user);
        }

        public void Save(WorkTime workTime)
        {
            _repository.Save(workTime);
        }

        public List<WorkTime> FindAll(User user, DateTime? startDate, DateTime? endDate)
        {
            var found = _repository.FindAll(user, startDate, endDate);
            if (_moduleService.CurrentWorkTime != null)
            {
                for (int i = 0; i < found.Count; i++)
                {
                    if (found[i].AggregateId == _moduleService.CurrentWorkTime.AggregateId)
                    {
                        found[i] = WorkTime.Combine(found[i], _moduleService.CurrentWorkTime);
                        break;
                    }
                }
            }
            return found;
        }

        public WorkTime? Find(User user, DateTime date)
        {
            var found = _repository.Find(user, date);
            if (found == null)
            {
                return null;
            }
            if (found.AggregateId == _moduleService.CurrentWorkTime?.AggregateId)
            {
                return WorkTime.Combine(found, _moduleService.CurrentWorkTime);
            }

            return found;
        }

        public WorkTime? FindFromSnapshot(WorkTimeSnapshotCreated snapshotEvent)
        {
            var found = _repository.FindFromSnapshot(snapshotEvent);
            if (found == null)
            {
                return null;
            }
            if (found.AggregateId == _moduleService.CurrentWorkTime?.AggregateId)
            {
                return WorkTime.Combine(found, _moduleService.CurrentWorkTime);
            }

            return found;

        }

        public WorkTime? FindLatestFromSnapshot(User user)
        {
            var found = _repository.FindLatestFromSnapshot(user);
            if (found == null)
            {
                return null;
            }
            if (found.AggregateId == _moduleService.CurrentWorkTime?.AggregateId)
            {
                return WorkTime.Combine(found, _moduleService.CurrentWorkTime);
            }

            return found;
        }

        public void Rollback(WorkTimeSnapshotCreated snapshot)
        {
            _repository.Rollback(snapshot);
        }
    }
}
