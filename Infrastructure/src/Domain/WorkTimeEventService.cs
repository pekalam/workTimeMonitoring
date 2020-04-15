using System.Collections.Generic;
using Infrastructure.Repositories;
using Infrastructure.WorkTimeAlg;

namespace Infrastructure.Domain
{
    public class WorkTimeEventService
    {
        private readonly IWorkTimeUow _workTimeUow;
        private readonly WorkTime _workTime;
        private readonly MouseKeyboardEventReducerService _mouseEventWindow =
            new MouseKeyboardEventReducerService(60_000, 1000);

        private readonly MouseKeyboardEventReducerService _keyboardEventWindow =
            new MouseKeyboardEventReducerService(60_000, 1000);

        private readonly List<MouseKeyboardEvent> _mouseEvents = new List<MouseKeyboardEvent>();
        private readonly List<MouseKeyboardEvent> _keyboardEvents = new List<MouseKeyboardEvent>();

        public WorkTimeEventService(IWorkTimeUow workTimeUow, WorkTime workTime)
        {
            _workTimeUow = workTimeUow;
            _workTime = workTime;
        }

        private void Save()
        {
            _workTimeUow.Save();
        }

        public void AddKeyboardEvent(MonitorEvent ev)
        {
            if (_keyboardEventWindow.AddEvent(ev, out var created))
            {
                _keyboardEvents.Add(created);
            }
        }

        public void DiscardChanges()
        {
            _workTimeUow.Rollback();
        }

        public void SaveChanges()
        {
            _workTimeUow.Commit();
        }
    }
}