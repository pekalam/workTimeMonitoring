using System;

namespace Domain.WorkTimeAggregate
{
    public class MouseKeyboardEvent
    {
        public DateTime Start { get; set; }
        public DateTime End { get; set; }
        public int[] EventsTimeline { get; set; }
        public int TotalTime { get; set; }
    }
}