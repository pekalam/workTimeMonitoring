using System;
using FluentAssertions;

namespace Domain.UnitTests
{
    public static class DateTimeTestExtentsions
    {
        public static void SafeCompare(this DateTime d1, DateTime d2)
        {
            d1.Subtract(d2).TotalSeconds.Should().BeLessThan(5);
        }
    }
}