using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using Domain.WorkTimeAggregate.Events;

[assembly: InternalsVisibleTo("Domain.UnitTests")]
[assembly: InternalsVisibleTo("Infrastructure.Tests")]

namespace Domain.WorkTimeAggregate
{
    internal static class InternalTimeService
    {
        public static Func<DateTime> GetCurrentDateTime = () => DateTime.UtcNow;
    }

    public class WorkTimeSnapshot
    {
        public DateTime? StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public DateTime DateCreated { get; set; }
        public User.User User { get; set; }
        public bool AutoStart { get; set; }
    }

    public partial class WorkTime
    {
        private IEnumerable<UserWorking> GetUserWorkingEvents()
        {
            if (_mouseActionEvents.Count == 0 && _keyboardActionEvents.Count == 0)
            {
                yield break;
            }

            DateTime start = _mouseActionEvents.First?.Value.MkEvent.Start ??
                             _keyboardActionEvents.First.Value.MkEvent.Start;

            while (true)
            {
                var firstMouse = _mouseActionEvents.FirstOrDefault(e => e.MkEvent.Start >= start);
                var firstKeyboard = _keyboardActionEvents.FirstOrDefault(e => e.MkEvent.Start >= start);

                FaceRecognitionFailure? overlappingFailure = null;

                if (firstKeyboard == null && firstMouse == null)
                {
                    yield break;
                }

                overlappingFailure = firstMouse != null
                    ? _recognitionFailureEvents.FirstOrDefault(e =>
                        firstMouse.MkEvent.Start >= e.Date &&
                        firstMouse.MkEvent.Start <= e.Date.AddMilliseconds(e.LengthMs))
                    : null;


                overlappingFailure = overlappingFailure == null && firstKeyboard != null
                    ? _recognitionFailureEvents.FirstOrDefault(e =>
                        firstKeyboard.MkEvent.Start >= e.Date &&
                        firstKeyboard.MkEvent.Start <= e.Date.AddMilliseconds(e.LengthMs))
                    : null;



                if (firstKeyboard != null && firstMouse != null)
                {
                    if (firstMouse.MkEvent.End < firstKeyboard.MkEvent.End)
                    {
                        start = firstKeyboard.MkEvent.Start;
                    }
                    else
                    {
                        start = firstMouse.MkEvent.Start;
                    }
                }
                else
                {
                    start = firstMouse?.MkEvent.Start ?? firstKeyboard.MkEvent.Start;
                }

                overlappingFailure = overlappingFailure == null
                    ? _recognitionFailureEvents.FirstOrDefault(e =>
                        start >= e.Date &&
                        start <= e.Date.AddMilliseconds(e.LengthMs))
                    : null;


                if (overlappingFailure != null)
                {
                    start = overlappingFailure.Date.AddMilliseconds(overlappingFailure.LengthMs);
                    continue;
                }


                DateTime? newStart = null;
                if (firstKeyboard != null)
                {
                    double idleTime = 0;
                    var current = _keyboardActionEvents.Find(firstKeyboard);
                    var next = current.Next;
                    while (next != null && !_recognitionFailureEvents.Any(e => next.Value.MkEvent.Start >= e.Date &&
                                                                               next.Value.MkEvent.Start <=
                                                                               e.Date.AddMilliseconds(e.LengthMs)))
                    {
                        idleTime = (long) (next.Value.MkEvent.Start - current.Value.MkEvent.End).TotalMilliseconds;
                        if (idleTime > 10_000d)
                        {
                            break;
                        }

                        current = next;
                        next = current.Next;
                    }

                    newStart = current.Value.MkEvent.End;
                }

                if (firstMouse != null)
                {
                    double idleTime = 0;
                    var current = _mouseActionEvents.Find(firstMouse);
                    var next = current.Next;
                    while (next != null && !_recognitionFailureEvents.Any(e => next.Value.MkEvent.Start >= e.Date &&
                                                                               next.Value.MkEvent.Start <=
                                                                               e.Date.AddMilliseconds(e.LengthMs)))
                    {
                        idleTime = (long) (next.Value.MkEvent.Start - current.Value.MkEvent.End).TotalMilliseconds;
                        if (idleTime > 10_000d)
                        {
                            break;
                        }

                        current = next;
                        next = current.Next;
                    }

                    if (newStart.HasValue)
                    {
                        if (current.Value.MkEvent.End > newStart.Value)
                        {
                            newStart = current.Value.MkEvent.End;
                        }
                    }
                    else
                    {
                        newStart = current.Value.MkEvent.End;
                    }
                }


                yield return new UserWorking(AggregateId, start, EventName.UserWorking, newStart.Value);

                start = newStart.Value;
            }
        }


        private IEnumerable<UserWatchingScreen> GetUserWatchingScreenEvents()
        {
            if (_mouseActionEvents.Count == 0 && _keyboardActionEvents.Count == 0)
            {
                yield break;
            }

            var currentMouse = _mouseActionEvents.First;
            var currentKeyboard = _keyboardActionEvents.First;

            while (currentKeyboard.Next != null || currentMouse.Next != null)
            {
                if ((currentMouse.Next.Value.MkEvent.Start - currentMouse.Value.MkEvent.End).TotalMilliseconds > 10_000)
                {
                    
                }
            }
        }
    }

    public partial class WorkTime
    {
        private readonly List<Event> _pendingEvents = new List<Event>();
        private readonly LinkedList<MouseAction> _mouseActionEvents = new LinkedList<MouseAction>();
        private readonly LinkedList<KeyboardAction> _keyboardActionEvents = new LinkedList<KeyboardAction>();
        private readonly List<FaceRecognitionFailure> _recognitionFailureEvents = new List<FaceRecognitionFailure>();
        private FaceRecognitionFailure? _faceRecognitionFailure;


        internal WorkTime(long aggregateId, User.User user, DateTime? startDate, DateTime endDate)
        {
            AggregateId = aggregateId;
            Create(user, startDate, endDate);
        }

        private WorkTime()
        {
        }


        public long AggregateVersion { get; private set; }
        public long AggregateId { get; private set; } = -1;
        public DateTime? StartDate { get; private set; }
        public DateTime EndDate { get; private set; }
        public DateTime DateCreated { get; private set; }
        public User.User User { get; private set; }
        public bool AutoStart { get; private set; }
        public bool Started => StartDate.HasValue;
        public bool FromSnapshot { get; private set; }

        public IReadOnlyList<Event> PendingEvents => _pendingEvents;
        public ICollection<MouseAction> MouseActionEvents => _mouseActionEvents;
        public ICollection<KeyboardAction> KeyboardActionEvents => _keyboardActionEvents;
        public IReadOnlyList<FaceRecognitionFailure> FaceRecognitionFailures => _recognitionFailureEvents;

        //implicit events
        public IEnumerable<UserWorking> UserWorkingEvents => GetUserWorkingEvents();
        public IEnumerable<UserWatchingScreen> UserWatchingScreenEvents => GetUserWatchingScreenEvents();

        private void AddEvent(Event ev)
        {
            ev.AggregateVersion = ++AggregateVersion;
            _pendingEvents.Add(ev);
        }


        private void Create(User.User user, DateTime? startDate, DateTime endDate)
        {
            StartDate = startDate;
            AutoStart = startDate.HasValue;
            User = user;
            EndDate = endDate;
            var now = InternalTimeService.GetCurrentDateTime();
            DateCreated = now;
            AddEvent(new WorkTimeCreated(AggregateId, now, StartDate, EndDate, DateCreated, User, AutoStart));
        }

        private void CheckIsStarted()
        {
            if (!StartDate.HasValue)
            {
                throw new Exception("WorkTime not started");
            }
        }

        private void StartWorkTime()
        {
            var now = InternalTimeService.GetCurrentDateTime();
            StartDate = now;
            AddEvent(new WorkTimeStarted(AggregateId, now, StartDate.Value));
        }

        public void StartManually()
        {
            if (AutoStart)
            {
                throw new Exception("Work time should be automatically started");
            }

            StartWorkTime();
        }

        public void Start()
        {
            if (!AutoStart)
            {
                throw new Exception("Work time not set to autoStart");
            }

            StartWorkTime();
        }

        internal void AddMouseAction(MouseKeyboardEvent mkEvent)
        {
            CheckIsStarted();

            var ev = new MouseAction(AggregateId, InternalTimeService.GetCurrentDateTime(), mkEvent);
            _mouseActionEvents.AddLast(ev);
            AddEvent(ev);
        }

        internal void AddKeyboardAction(MouseKeyboardEvent mkEvent)
        {
            CheckIsStarted();

            var ev = new KeyboardAction(AggregateId, InternalTimeService.GetCurrentDateTime(), mkEvent);
            _keyboardActionEvents.AddLast(ev);
            AddEvent(ev);
        }

        internal void ClearEvents()
        {
            _keyboardActionEvents.Clear();
            _mouseActionEvents.Clear();
            _recognitionFailureEvents.Clear();
        }

        internal void StartRecognitionFailure(bool faceDetected, bool faceRecognized)
        {
            CheckIsStarted();
            if (_faceRecognitionFailure != null)
            {
                throw new Exception("Previous faceRecognitionFailure not stopped");
            }

            _faceRecognitionFailure = new FaceRecognitionFailure(AggregateId, InternalTimeService.GetCurrentDateTime(),
                faceRecognized, faceDetected);
        }

        internal void StopRecognitionFailure()
        {
            CheckIsStarted();
            if (_faceRecognitionFailure == null)
            {
                throw new Exception("faceRecognitionFailure not started");
            }

            _faceRecognitionFailure.LengthMs =
                (long) (InternalTimeService.GetCurrentDateTime() - _faceRecognitionFailure.Date).TotalMilliseconds;

            _recognitionFailureEvents.Add(_faceRecognitionFailure);
            AddEvent(_faceRecognitionFailure);
            _faceRecognitionFailure = null;
        }

        public WorkTimeSnapshotCreated TakeSnapshot()
        {
            ClearEvents();
            var snapshot = new WorkTimeSnapshot()
            {
                StartDate = StartDate,
                User = User,
                EndDate = EndDate,
                AutoStart = AutoStart,
                DateCreated = DateCreated,
            };

            var ev = new WorkTimeSnapshotCreated(AggregateId, InternalTimeService.GetCurrentDateTime(), snapshot);
            AddEvent(ev);
            return ev;
        }

        public void RollbackToSnapshot(WorkTimeSnapshotCreated workTimeSnapshotEvent)
        {
            CheckIsStarted();
            if (workTimeSnapshotEvent.AggregateId != AggregateId)
            {
                throw new Exception("Invalid snapshot aggregateId");
            }

            AggregateVersion = workTimeSnapshotEvent.AggregateVersion;
            FromSnapshot = true;
            _pendingEvents.Clear();
            ClearEvents();

            Apply(workTimeSnapshotEvent);
        }
    }
}