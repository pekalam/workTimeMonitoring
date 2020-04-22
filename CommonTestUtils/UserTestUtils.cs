using System;
using System.Diagnostics;
using System.Threading;
using Domain.User;

namespace DomainTestUtils
{
    public static class UserTestUtils
    {
        private static int _id = Int32.MaxValue;


        public static User CreateTestUser()
        {
            Debug.WriteLine("u " + _id);
            return new User(Interlocked.Decrement(ref _id), new Username("mpekala"));
        }

        public static User CreateTestUser(long id)
        {
            return new User(id, new Username("mpekala"));
        }
    }
}