using System;

namespace Domain.WorkTimeAggregate.Events
{
    public sealed class MonitorEvent
    {
        public int TotalTimeMs;
        public DateTime EventStart;
        public string? Executable;
    }
}