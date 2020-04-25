using System;
using Domain.Services;
using Infrastructure.Services;

namespace Infrastructure.Tests
{
    static class TestUtils
    {
        public static int AsInt(this string str)
        {
            return Convert.ToInt32(str);
        }

        public static IConfigurationService ConfigurationService => new ConfigurationService("testsettings.json");
    }
}