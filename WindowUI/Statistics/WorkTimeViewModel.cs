using Domain.WorkTimeAggregate;
using System;

namespace WindowUI.Statistics
{
    public class WorkTimeViewModel
    {
        public WorkTimeViewModel(WorkTime workTime)
        {
            Start = workTime.StartDate?.ToLocalTime();
            End = workTime.EndDate.ToLocalTime();
            InProgress = workTime.Started && !workTime.Stopped && !workTime.Paused;
        }

        public DateTime? Start { get; set; }
        public DateTime End { get; set; }
        public bool InProgress { get; set; }
    }
}
