using System;
using System.Linq;
using AutoMapper;
using Domain.WorkTimeAggregate.Events;
using DomainTestUtils;
using FluentAssertions;
using Infrastructure.Db;
using Infrastructure.Services;
using OpenCvSharp;
using WMAlghorithm;
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

        static DbMappingTests()
        {
            SharedFaceRecognitionModel.Init(new ConfigurationService(""));
        }

        [Fact]
        public void Event_DbEvent()
        {
            var worktime = WorkTimeTestUtils.CreateManual();

            var ev = worktime.PendingEvents.First() as WorkTimeCreated;

            var dbEvent = _mapper.Map<DbEvent>(ev);

            dbEvent.Id.Should().BeNull();
            dbEvent.AggregateId.Should().Be(ev.AggregateId);
            dbEvent.AggregateVersion.Should().Be(ev.AggregateVersion);
            dbEvent.Data.Should().NotBeNullOrWhiteSpace();
            dbEvent.Data.Should().NotContain("AggregateVersion");
            dbEvent.Data.Should().NotContain("AggregateId");
            dbEvent.Date.Should().Be(ev.Date);

            var ev1 = _mapper.Map<Event>(dbEvent) as WorkTimeCreated;

            ev1.Should().BeEquivalentTo(ev);
        }

        [Fact]
        public void TestImage_to_DbTestImage()
        {
            var utils = new ImageTestUtils();
            var (rect, face) = utils.GetFaceImg("front");

            var encodings = new DnFaceRecognition().GetFaceEncodings(face);

            var now = DateTime.UtcNow;
            var testImg = new TestImage(encodings, new Rect(0, 1, 20, 21), Mat.Zeros(4, 4, MatType.CV_8UC1), HeadRotation.Left, now, true, UserTestUtils.CreateTestUser(1).UserId);
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
            dbTestImg.UserId.Should().Be(1);
        }
    }
}
