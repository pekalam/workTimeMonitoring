using System;
using System.Collections.Generic;
using System.Linq;
using Domain.WorkTimeAggregate.Events;

namespace Domain.WorkTimeAggregate
{
    internal class MouseKeyboardEventBuilder
    {
        private readonly List<MonitorEvent> _currentTimeLineEvents = new List<MonitorEvent>();
        private readonly List<int> _timeline = new List<int>();

        private readonly int _windowSizeMs;
        private readonly int _monitorEventWindowSizeMs;

        private int? _currentTimeLineStart;
        private DateTime? _start;

        public MouseKeyboardEventBuilder(int windowSizeMs, int monitorEventWindowSizeMs)
        {
            _windowSizeMs = windowSizeMs;
            _monitorEventWindowSizeMs = monitorEventWindowSizeMs;
        }

        private int SumTime()
        {
            int sum = 0;
            for (int i = 1; i < _timeline.Count; i += 2)
            {
                sum += _timeline[i] - _timeline[i - 1];
            }

            return sum;
        }

        private void AddCurrentTimeline()
        {
            _timeline.Add(_currentTimeLineStart.Value);
            _timeline.Add(
                _currentTimeLineEvents.Aggregate(_currentTimeLineStart.Value, (sum, e) => sum + e.TotalTimeMs));
        }

        private MouseKeyboardEvent BuildEvent()
        {
            if (_currentTimeLineEvents.Count > 0)
            {
                AddCurrentTimeline();
            }

            var ev = new MouseKeyboardEvent()
            {
                EventsTimeline = _timeline.ToArray(),
                Start = _start.Value,
                End = _start.Value.AddMilliseconds(_timeline.Last()),
                TotalTime = SumTime(),
            };
            Reset();
            return ev;
        }

        public void Reset()
        {
            _currentTimeLineStart = null;
            _currentTimeLineEvents.Clear();
            _timeline.Clear();
            _start = null;
        }

        private void AddNewEvent(MonitorEvent ev)
        {
            int offset = (int) (ev.EventStart - _start.Value).TotalMilliseconds;
            int end = offset + ev.TotalTimeMs;

            if (_currentTimeLineStart.HasValue)
            {
                if (offset >= _currentTimeLineStart.Value + _monitorEventWindowSizeMs
                    || end >= _currentTimeLineStart.Value + _monitorEventWindowSizeMs)
                {
                    AddCurrentTimeline();

                    _currentTimeLineEvents.Clear();
                    _currentTimeLineStart = offset;
                    _currentTimeLineEvents.Add(ev);
                }
                else
                {
                    _currentTimeLineEvents.Add(ev);
                }
            }
            else
            {
                if (end >= _monitorEventWindowSizeMs + offset)
                {
                    _timeline.Add(offset);
                    _timeline.Add(offset + ev.TotalTimeMs);
                    _currentTimeLineStart = null;
                }
                else
                {
                    _currentTimeLineStart = offset;
                    _currentTimeLineEvents.Add(ev);
                }
            }
        }

        public bool AddEvent(MonitorEvent ev, out MouseKeyboardEvent created)
        {
            var ret = false;

            if (!_start.HasValue)
            {
                _start = ev.EventStart;
                created = null;
            }
            else
            {
                var start = ev.EventStart - _start.Value;
                if (start.TotalMilliseconds >= _windowSizeMs)
                {
                    created = BuildEvent();
                    _start = ev.EventStart;
                    ret = true;
                }
                else
                {
                    created = null;
                }
            }

            AddNewEvent(ev);
            return ret;
        }

        public MouseKeyboardEvent? Flush()
        {
            if (_timeline.Count > 0)
            {
                return BuildEvent();
            }

            return null;
        }
    }
}