using System;

namespace Infrastructure.Tests
{
    static class TestUtilsHelpers
    {
        public static int AsInt(this string str)
        {
            return Convert.ToInt32(str);
        }
    }
}