using FluentAssertions;
using System;

namespace Domain.UnitTests
{
    public static class DateTimeTestExtentsions
    {
        public static void SafeCompare(this DateTime d1, DateTime d2, double ms = 5000)
        {
            d1.Subtract(d2).TotalMilliseconds.Should().BeLessThan(ms);
        }
    }
}