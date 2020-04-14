using System;
using System.Data.SQLite;
using Dapper;
using Infrastructure.Db;
using Infrastructure.Repositories;
using Infrastructure.Services;
using Infrastructure.WorkTimeAlg;

namespace UnitTests
{
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
}