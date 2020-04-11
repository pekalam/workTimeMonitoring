using System;
using OpenCvSharp;

namespace Infrastructure.WorkTime
{
    public class TestImage
    {
        public TestImage(FaceImg faceGrayscale, FaceImg faceColor)
        {
            FaceGrayscale = faceGrayscale;
            FaceColor = faceColor;
        }

        public FaceImg FaceGrayscale { get; }
        public FaceImg FaceColor { get; }

        public static TestImage CreateFromFace(Mat colorImg)
        {
            var faceColor = FaceImg.CreateColor(colorImg);
            var faceGrayscale = FaceImg.CreateGrayscale(colorImg);
            return new TestImage(faceGrayscale, faceColor);
        }
    }
}