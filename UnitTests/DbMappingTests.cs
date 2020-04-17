using System;
using System.Linq;
using AutoMapper;
using DomainTestUtils;
using FluentAssertions;
using Infrastructure.Db;
using OpenCvSharp;
using WorkTimeAlghorithm;
using Xunit;

namespace Infrastructure.Tests
{
    [Trait("Category", "Integration")]
    public class DbMappingTests
    {
        private IMapper _mapper;

        public DbMappingTests()
        {
            _mapper = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile<DbTestImageProfile>();
                cfg.AddProfile<DbEventProfile>();
            }).CreateMapper();
        }

        [Fact]
        public void f()
        {
            var worktime = WorkTimeTestUtils.CreateManual();

            var ev = worktime.PendingEvents.First();

            var dbEvent = _mapper.Map<DbEvent>(ev);

            dbEvent.Id.Should().BeNull();
            dbEvent.AggregateId.Should().Be(ev.AggregateId);
            dbEvent.AggregateVersion.Should().Be(ev.AggregateVersion);
            dbEvent.Data.Should().NotBeNullOrWhiteSpace();
            dbEvent.Date.Should().Be(ev.Date);
        }

        [Fact]
        public void TestImageMapping()
        {
            var utils = new ImageTestUtils();
            var (rect, face) = utils.GetFaceImg("front");

            var encodings = new DnFaceRecognition().GetFaceEncodings(face);

            var now = DateTime.UtcNow;
            var testImg = new TestImage(encodings, new Rect(0, 1, 20, 21), Mat.Zeros(4, 4, MatType.CV_8UC1), HeadRotation.Left, now, true);
            var dbTestImg = _mapper.Map<DbTestImage>(testImg);

            dbTestImg.Img.Should().NotBeEmpty();
            dbTestImg.FaceEncoding.Should().NotBeEmpty();
            dbTestImg.FaceLocation_x.Should().Be(0);
            dbTestImg.FaceLocation_y.Should().Be(1);
            dbTestImg.FaceLocation_width.Should().Be(20);
            dbTestImg.FaceLocation_height.Should().Be(21);
            dbTestImg.DateCreated.Should().Be(now);
            dbTestImg.IsReferenceImg.Should().BeTrue();
            dbTestImg.HorizontalHeadRotation.Should().Be((int)HeadRotation.Left);
            dbTestImg.Id.Should().BeNull();
        }
    }
}
