using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;
using System.Runtime.CompilerServices;
using AutoMapper;
using Dapper;
using Domain;
using Domain.Services;
using Infrastructure.Db;
using Infrastructure.Services;
using WorkTimeAlghorithm;

[assembly: InternalsVisibleTo("Infrastructure.Tests")]
namespace Infrastructure.Repositories
{
    internal class SqLiteTestImageRepository : ITestImageRepository
    {
        public const string TestImageTable = "TestImage";
        private const string ColumnNames = "Id, FaceLocation_x, FaceLocation_y, FaceLocation_width, FaceLocation_height, Img, HorizontalHeadRotation, DateCreated, FaceEncoding, IsReferenceImg";

        private readonly SqliteSettings _settings;
        private readonly IMapper _mapper;

        static SqLiteTestImageRepository()
        {
            SqlMapper.RemoveTypeMap(typeof(DateTime));
            SqlMapper.AddTypeHandler(new DateTimeHandler());
        }

        public SqLiteTestImageRepository(IConfigurationService configurationService, IMapper mapper)
        {
            _settings = configurationService.Get<SqliteSettings>("sqlite");
            _mapper = mapper;
        }

        private SQLiteConnection CreateConnection()
        {
            return new SQLiteConnection(_settings.ConnectionString);
        }

        private int CountQuery()
        {
            var sql = @$"SELECT COUNT(*) FROM {TestImageTable};";
            using var connection = CreateConnection();
            return connection.ExecuteScalar<int>(sql);
        }

        private void CheckNotNull(TestImage img)
        {
            if (img == null)
            {
                throw new NullReferenceException("Null testImage");
            }
        }

        private void CheckHasBeenAdded(TestImage img)
        {
            if (!img.Id.HasValue)
            {
                throw new Exception($"Img not in db");
            }
        }

        public int Count => CountQuery();

        public IReadOnlyList<TestImage> GetAll()
        {
            var sql =
                @$"SELECT {ColumnNames}
                   FROM {TestImageTable};";

            using var connection = CreateConnection();
            var imgs = connection.Query<DbTestImage>(sql).Select(i => _mapper.Map<TestImage>(i));
            return imgs.ToList();
        }

        public IReadOnlyList<TestImage> GetReferenceImages()
        {
            var sql =
                @$"SELECT {ColumnNames}
                   FROM {TestImageTable}
                   WHERE IsReferenceImg = true;";

            using var connection = CreateConnection();
            var imgs = connection.Query<DbTestImage>(sql).Select(i => _mapper.Map<TestImage>(i));
            return imgs.ToList();
        }

        public IReadOnlyList<TestImage> GetMostRecentImages(DateTime startDate, int maxCount)
        {
            if (maxCount <= 0)
            {
                throw new ArgumentException($"Invalid maxCount {maxCount}");
            }

            var sql =
                @$"SELECT {ColumnNames}
                   FROM {TestImageTable}
                   WHERE DateCreated >= @startDate  
                   ORDER BY DateCreated DESC LIMIT @maxCount;";

            using var connection = CreateConnection();
            var imgs = connection.Query<DbTestImage>(sql, new { startDate, maxCount }).Select(i => _mapper.Map<TestImage>(i));
            return imgs.ToList();
        }

        public TestImage Add(TestImage img)
        {
            CheckNotNull(img);

            var dbEntity = _mapper.Map<DbTestImage>(img);
            var sql =
                $@"INSERT INTO TestImage VALUES ( @Id, @FaceLocation_x, @FaceLocation_y, @FaceLocation_width, @FaceLocation_height, @Img, @HorizontalHeadRotation, @DateCreated, @FaceEncoding, @IsReferenceImg ); SELECT last_insert_rowid();";

            using var connection = CreateConnection();
            var result = connection.ExecuteScalar<int>(sql, dbEntity);

            if (result <= 0)
            {
                throw new Exception("Not inserted");
            }

            TestImageDeserializationHelper.SetInternalFields(result, img);
            return img;
        }

        public void Remove(TestImage img)
        {
            CheckNotNull(img);
            CheckHasBeenAdded(img);

            var sql = $@"DELETE FROM {TestImageTable} WHERE Id=@Id;";

            using var connection = CreateConnection();
            var result = connection.Execute(sql, img);

            if (result != 1)
            {
                throw new Exception($"Could not remove testImage {img.Id}");
            }
        }

        public void Clear()
        {
            using var connection = CreateConnection();
            connection.Execute($"DELETE FROM {SqLiteTestImageRepository.TestImageTable};");
        }
    }
}