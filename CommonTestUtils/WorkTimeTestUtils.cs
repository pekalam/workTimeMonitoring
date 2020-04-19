using System;
using Domain;
using Domain.User;
using Domain.WorkTimeAggregate;
using Serilog;

namespace DomainTestUtils
{
    public static class WorkTimeTestUtils
    {
        private static int _id = Int32.MaxValue;

        public static WorkTime CreateManual()
        {
            return new WorkTime(_id--, UserTestUtils.CreateTestUser(), null, DateTime.UtcNow.AddMinutes(10));
        }

        public static WorkTime CreateManual(User user)
        {
            return new WorkTime(_id--, user, null, DateTime.UtcNow.AddMinutes(10));
        }

        public static WorkTime CreateStartedManually()
        {
            var workTime = new WorkTime(_id--, UserTestUtils.CreateTestUser(), null, DateTime.UtcNow.AddMinutes(10));
            workTime.StartManually();
            workTime.MarkPendingEventsAsHandled();
            return workTime;
        }

    }
}