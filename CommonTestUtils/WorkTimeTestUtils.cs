using Domain.User;
using Domain.WorkTimeAggregate;
using System;
using System.Diagnostics;
using System.Threading;

namespace DomainTestUtils
{
    public static class WorkTimeTestUtils
    {
        private static int _id = Int32.MaxValue;

        public static WorkTime CreateManual()
        {
            Debug.WriteLine(_id);
            return new WorkTime(Interlocked.Decrement(ref _id), UserTestUtils.CreateTestUser(), null, InternalTimeService.GetCurrentDateTime().AddMinutes(10));
        }

        public static WorkTime CreateManual(User user)
        {
            Debug.WriteLine(_id);
            return new WorkTime(Interlocked.Decrement(ref _id), user, null, InternalTimeService.GetCurrentDateTime().AddMinutes(10));
        }

        public static WorkTime CreateStartedManually()
        {
            var workTime = new WorkTime(_id--, UserTestUtils.CreateTestUser(), null, InternalTimeService.GetCurrentDateTime().AddMinutes(10));
            workTime.StartManually();
            workTime.MarkPendingEventsAsHandled();
            return workTime;
        }

    }
}