using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FluentAssertions;
using Infrastructure.Services;
using Infrastructure.WorkTime;
using OpenCvSharp;
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

    public class DefaultTestImageRepositoryTests : TestImageRepositoryTests
    {
        public override ITestImageRepository GetTestImageRepository()
        {
            return new DefaultTestImageRepository();
        }
    }

    public abstract class TestImageRepositoryTests
    {
        private ITestImageRepository _repository;

        public TestImageRepositoryTests()
        {
            _repository = GetTestImageRepository();
        }

        public abstract ITestImageRepository GetTestImageRepository();

        private TestImage CreateTestImage()
        {
            return new TestImage(FaceImg.CreateGrayscale(Mat.Zeros(type: MatType.CV_8UC3,rows: 200,cols: 200)),
                FaceImg.CreateColor(Mat.Zeros(type: MatType.CV_8UC3, rows: 200, cols: 200)));
        }

        [Fact]
        public void Add_when_valid_adds_to_repo()
        {
            var t1 = CreateTestImage();
            var t2 = CreateTestImage();
            _repository.Add(t1);
            _repository.Count.Should().Be(1);

            _repository.Add(t2);
            _repository.Count.Should().Be(2);
        }

        [Fact]
        public void Add_when_null_throws()
        {
            Assert.Throws<NullReferenceException>(() => _repository.Add(null));
        }

        [Fact]
        public void f()
        {
            var t1 = CreateTestImage();
            var t2 = CreateTestImage();
            _repository.Add(t2);
            _repository.Add(t1);

            _repository.Remove(t1);
            _repository.Count.Should().Be(1);
        }


        [Fact]
        public void f2()
        {
            var t1 = CreateTestImage();

            Assert.Throws<Exception>(() => _repository.Remove(t1));
        }

        [Fact]
        public void f3()
        {
            var t1 = CreateTestImage();
            _repository.Add(t1);
            Assert.Throws<NullReferenceException>(() => _repository.Remove(null));
        }

        [Fact]
        public void g()
        {
            var t1 = CreateTestImage();
            var t2 = CreateTestImage();
            _repository.Add(t1);
            _repository.Add(t2);

            _repository.GetAll().Count.Should().Be(2);
            _repository.GetAll().First().Should().Be(t1);
            _repository.GetAll().Last().Should().Be(t2);
        }
    }
}
