using System;
using System.Data.SQLite;
using Dapper;
using Domain.User;
using DomainTestUtils;
using Infrastructure.Db;
using Infrastructure.Repositories;
using Infrastructure.Services;
using WorkTimeAlghorithm;
using Xunit;

namespace Infrastructure.Tests
{
    [Trait("Category", "Integration")]
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
            return new SqliteTestImageRepository(new ConfigurationService(""),
                MapperConfigFactory.Create().CreateMapper());
        }

        protected override TestImage CreateTestImage(bool isReferenceImg = true)
        {
            var utils = new ImageTestUtils();
            var (rect, face) = utils.GetFaceImg("front");
            var user = UserTestUtils.CreateTestUser(1);

            return new TestImage(FaceEncodings, rect, face, HeadRotation.Front, DateTime.UtcNow, isReferenceImg, user.UserId);
        }

        protected override User CreateUser()
        {
            return UserTestUtils.CreateTestUser(1);
        }

        public void Dispose()
        {
            SqliteTestUtils.TruncTable(SqliteTestImageRepository.TestImageTable);
        }



    }
}