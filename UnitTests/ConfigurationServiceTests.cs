using System;
using FluentAssertions;
using Infrastructure.Services;
using WorkTimeAlghorithm;
using Xunit;

namespace Infrastructure.Tests
{
    [Trait("Category", "Integration")]
    public class ConfigurationServiceTests
    {
        [Fact]
        public void Get_returns_default_instance_if_file_not_found()
        {
            var service = new ConfigurationService("");
            var settings = service.Get<HeadPositionServiceSettings>("");
            settings.Should().NotBeNull();
            settings.GetType().Should().Be<HeadPositionServiceSettings>();
        }

        [Fact]
        public void Get_if_section_exists_returns_valid_instance()
        {
            var service = new ConfigurationService("settings.json");
            var settings = service.Get<HeadPositionServiceSettings>(nameof(HeadPositionServiceSettings));

            settings.Should().NotBeNull();
            settings.HorizontalPoseThreshold.Should().Be(1);
        }

        [Fact]
        public void Get_if_section_not_found_returns_default_instance()
        {
            var service = new ConfigurationService("settings.json");
            var instance = service.Get<HeadPositionServiceSettings>("x");
            instance.Should().BeEquivalentTo(new HeadPositionServiceSettings());
        }
    }
}