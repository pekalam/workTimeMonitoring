using System;
using System.Linq;
using System.Threading.Tasks;
using Domain.User;
using FluentAssertions;
using OpenCvSharp;
using WMAlghorithm;
using Xunit;

namespace Infrastructure.Tests
{
    public abstract class TestImageRepositoryTests
    {
        private readonly ITestImageRepository _repository;
        private User user;

        public TestImageRepositoryTests()
        {
            _repository = GetTestImageRepository();
            user = CreateUser();
        }

        public abstract ITestImageRepository GetTestImageRepository();

        protected abstract TestImage CreateTestImage(bool isReferenceImg = true);

        protected abstract User CreateUser();

        private void CompareTestImgs(TestImage t1, TestImage t2)
        {
            t1.Should().BeEquivalentTo(t2, opt => opt.Excluding(i => i.Img));
            var mat = t1.Img;

            mat.Rows.Should().Be(t2.Img.Rows);
            mat.Cols.Should().Be(t2.Img.Cols);
        }

        [Fact]
        public void Add_when_valid_adds_to_repo()
        {
            var t1 = CreateTestImage();
            var t2 = CreateTestImage();
            _repository.Add(t1);
            _repository.Count(user).Should().Be(1);

            _repository.Add(t2);
            _repository.Count(user).Should().Be(2);
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
            _repository.Count(user).Should().Be(2);

            _repository.Remove(t1);
            _repository.Count(user).Should().Be(1);
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
            _repository.Count(user).Should().Be(2);

            var all = _repository.GetAll(user);
            all.Count().Should().Be(2);
            CompareTestImgs(all.First(), t1);
            CompareTestImgs(all.Last(), t2);
        }

        [Fact]
        public void Clear_removes_all_data()
        {
            var t1 = CreateTestImage();
            var t2 = CreateTestImage();
            _repository.Add(t1);
            _repository.Add(t2);
            _repository.Count(user).Should().Be(2);

            _repository.Clear(user);

            _repository.Count(user).Should().Be(0);
        }

        [Fact]
        public void GetReferenceImgs_returns_only_referenceImgs()
        {
            var t1 = CreateTestImage(false);
            var t2 = CreateTestImage(true);
            var t3 = CreateTestImage(true);
            _repository.Add(t1);
            _repository.Add(t2);
            _repository.Add(t3);
            _repository.Count(user).Should().Be(3);


            var result = _repository.GetReferenceImages(user);

            result.Count().Should().Be(2);
            CompareTestImgs(result.First(), t2);
            CompareTestImgs(result.Last(), t3);
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

            var result = _repository.GetMostRecentImages(user, now, 1);

            result.Count().Should().Be(1);
            CompareTestImgs(result.First(), t2);

            await Task.Delay(200);
            now = DateTime.UtcNow;
            var t3 = CreateTestImage(true);
            _repository.Add(t3);

            result = _repository.GetMostRecentImages(user, now, 1);
            CompareTestImgs(result.First(), t3);
        }
    }
}