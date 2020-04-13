using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using Dapper;
using DlibDotNet;
using FaceRecognitionDotNet;
using FluentAssertions;
using Infrastructure.Db;
using Infrastructure.Repositories;
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

    public class SqliteTestImageRepositoryTests : TestImageRepositoryTests, IDisposable
    {
        private static FaceEncodingData? FaceEncodings;

        static SqliteTestImageRepositoryTests()
        {
            var utils = new ImageTestUtils();
            var (rect, face) = utils.GetFaceImg("front");

            FaceEncodings = new DnFaceRecognition().GetFaceEncodings(face);
        }

        public override ITestImageRepository GetTestImageRepository()
        {
            return new SqLiteTestImageRepository(new ConfigurationService(""),
                MapperConfigFactory.Create().CreateMapper());
        }

        protected override TestImage CreateTestImage(bool isReferenceImg = true)
        {
            var utils = new ImageTestUtils();
            var (rect, face) = utils.GetFaceImg("front");

            return new TestImage(FaceEncodings, rect, face, HeadRotation.Front, DateTime.UtcNow, isReferenceImg);
        }

        public void Dispose()
        {
            using var connection = new SQLiteConnection(new ConfigurationService("").Get<SqliteSettings>("sqlite").ConnectionString);
            connection.Execute($"DELETE FROM {SqLiteTestImageRepository.TestImageTable};");
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

        protected  virtual TestImage CreateTestImage(bool isReferenceImg = true)
        {
            return new TestImage( new Rect(0, 0, 20, 20), Mat.Zeros(4, 4, MatType.CV_8UC1), HeadRotation.Left, DateTime.UtcNow, isReferenceImg);
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
        public void Remove_removes_existing_img()
        {
            var t1 = CreateTestImage();
            var t2 = CreateTestImage();
            _repository.Add(t2);
            _repository.Add(t1);

            _repository.Remove(t1);
            _repository.Count.Should().Be(1);
        }


        [Fact]
        public void Remove_if_does_not_exist_throws()
        {
            var t1 = CreateTestImage();

            Assert.Throws<Exception>(() => _repository.Remove(t1));
        }

        [Fact]
        public void Remove_if_null_param_throws()
        {
            var t1 = CreateTestImage();
            _repository.Add(t1);
            Assert.Throws<NullReferenceException>(() => _repository.Remove(null));
        }

        [Fact]
        public void GetAll_returns_all_imgs()
        {
            var t1 = CreateTestImage();
            var t2 = CreateTestImage();
            _repository.Add(t1);
            _repository.Add(t2);

            _repository.GetAll().Count.Should().Be(2);
            var x = _repository.GetAll();
            _repository.GetAll().First().Id.Should().Be(t1.Id);
            _repository.GetAll().Last().Id.Should().Be(t2.Id);
        }

        [Fact]
        public void Clear_removes_all_data()
        {
            var t1 = CreateTestImage();
            var t2 = CreateTestImage();
            _repository.Add(t1);
            _repository.Add(t2);

            _repository.Clear();

            _repository.Count.Should().Be(0);
        }

        [Fact]
        public void GetReferenceImgs_returns_only_referenceImgs()
        {
            var t1 = CreateTestImage(false);
            var t2 = CreateTestImage(true);
            _repository.Add(t1);
            _repository.Add(t2);


            var result = _repository.GetReferenceImages();

            result.Count.Should().Be(1);
            result.First().Id.Should().Be(t2.Id);
        }


        [Fact]
        public async Task GetRecentImgs_returns_only_recent()
        {
            DateTime now = DateTime.UtcNow;

            var t1 = CreateTestImage(false);
            await Task.Delay(200);
            var t2 = CreateTestImage(true);
            _repository.Add(t1);
            _repository.Add(t2);

            await Task.Delay(500);

            var result = _repository.GetMostRecentImages(now, 1);

            result.Count.Should().Be(1);
            result.First().Id.Should().Be(t2.Id);
        }
    }
}