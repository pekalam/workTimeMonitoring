using System;
using FluentAssertions;
using Infrastructure.Services;
using Infrastructure.WorkTimeAlg;
using Xunit;

namespace UnitTests
{
    public class ConfigurationServiceTests
    {
        [Fact]
        public void f()
        {
            var service = new ConfigurationService("");
            var settings = service.Get<HeadPositionServiceSettings>("");
            settings.Should().NotBeNull();
            settings.GetType().Should().Be<HeadPositionServiceSettings>();
        }

        [Fact]
        public void g()
        {
            var service = new ConfigurationService("settings.json");
            var settings = service.Get<HeadPositionServiceSettings>(nameof(HeadPositionServiceSettings));

            settings.Should().NotBeNull();
            settings.HorizontalPoseThreshold.Should().Be(1);
        }

        [Fact]
        public void n()
        {
            var service = new ConfigurationService("settings.json");
            Assert.Throws<NullReferenceException>(() => service.Get<HeadPositionServiceSettings>("x"));
        }
    }
}