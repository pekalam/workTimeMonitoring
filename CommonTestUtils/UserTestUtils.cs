using System;
using Domain.User;

namespace DomainTestUtils
{
    public static class UserTestUtils
    {
        private static int _id = Int32.MaxValue;


        public static User CreateTestUser()
        {
            return new User(_id--, new Username("mpekala"));
        }

        public static User CreateTestUser(long id)
        {
            return new User(id, new Username("mpekala"));
        }
    }
}