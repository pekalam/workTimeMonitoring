using System;

namespace Domain.WorkTime.Events
{
    public sealed class MonitorEvent
    {
        public int TotalTimeMs;
        public DateTime EventStart;
    }
}