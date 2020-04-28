using System;
using System.Collections.Generic;
using System.Text;
using Domain.WorkTimeAggregate;

namespace WindowUI.Statistics
{
    public class WorkTimeViewModel
    {
        public WorkTimeViewModel(WorkTime workTime)
        {
            Start = workTime.StartDate;
            End = workTime.EndDate;
            InProgress = workTime.Started && !workTime.Stopped && !workTime.Paused;
        }

        public DateTime? Start { get; set; }
        public DateTime End { get; set; }
        public bool InProgress { get; set; }
    }
}
