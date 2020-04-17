using System;
using Domain;
using Domain.User;
using Domain.WorkTime;

namespace DomainTestUtils
{


    public static class UserTestUtils
    {
        public static User CreateTestUser()
        {
            return new User(new Username("mpekala"));
        }

    }

    public static class WorkTimeTestUtils
    {
        public static WorkTime CreateManual()
        {
            return new WorkTime(1, UserTestUtils.CreateTestUser(), null, DateTime.UtcNow.AddMinutes(10));
        }

        public static WorkTime CreateStartedManually()
        {
            var workTime = new WorkTime(1, UserTestUtils.CreateTestUser(), null, DateTime.UtcNow.AddMinutes(10));
            workTime.StartManually();
            workTime.MarkPendingEventsAsHandled();
            return workTime;
        }

    }
}