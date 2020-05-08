using AutoMapper;
using OpenCvSharp;
using WMAlghorithm;

namespace Infrastructure.Db
{
    internal class DbTestImageProfile : Profile
    {
        public DbTestImageProfile()
        {
            CreateMap<TestImage, DbTestImage>()
                .ForMember(db => db.Img, opt => { opt.MapFrom<byte[]>((testImage, dbEnt) => testImage.Img.ToBytes()); })
                .ForMember(db => db.FaceEncoding,
                    opt =>
                    {
                        opt.MapFrom<byte[]>((testImage, _) =>
                            FaceEncodingHelpers.Serialize(testImage.FaceEncoding.Value));
                    })
                .ForMember(db => db.FaceLocation_x, opt => opt.MapFrom<int>(image => image.FaceLocation.X))
                .ForMember(db => db.FaceLocation_y, opt => opt.MapFrom<int>(image => image.FaceLocation.Y))
                .ForMember(db => db.FaceLocation_width, opt => { opt.MapFrom<int>(image => image.FaceLocation.Width); })
                .ForMember(db => db.FaceLocation_height, opt => opt.MapFrom<int>(image => image.FaceLocation.Height));
                

            CreateMap<DbTestImage, TestImage>()
                .ForMember(image => image.FaceEncoding, opt =>
                {
                    opt.MapFrom<FaceEncodingData>((db, _) =>
                    {
                        var encoding = FaceEncodingHelpers.Deserialize(db.FaceEncoding);
                        return new FaceEncodingData(encoding);
                    });
                })
                .ForMember(image => image.Img, opt =>
                {
                    opt.MapFrom<Mat>((db, _) =>
                    {
                        var mat = Mat.FromImageData(db.Img);
                        return mat;
                    });
                })
                .ForMember(image => image.FaceLocation,
                    opt =>
                    {
                        opt.MapFrom<Rect>((db, _) => new Rect(db.FaceLocation_x, db.FaceLocation_y,
                            db.FaceLocation_width, db.FaceLocation_height));
                    });
        }
    }
}