using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using DlibDotNet;
using FaceRecognitionDotNet;
using FluentAssertions;
using Infrastructure.Repositories;
using Infrastructure.WorkTimeAlg;
using OpenCvSharp;
using Xunit;

namespace UnitTests
{
    public abstract class TestImageRepositoryTests
    {
        private readonly ITestImageRepository _repository;

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