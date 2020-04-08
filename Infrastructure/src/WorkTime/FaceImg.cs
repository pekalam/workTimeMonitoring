using System;
using OpenCvSharp;

namespace Infrastructure.WorkTime
{

    public class FaceImg
    {
        public const int Width = 92;
        public const int Height = 112;

        public Mat Img { get; }

        public FaceImg(Mat img)
        {
            if (img.Empty())
            {
                throw new Exception("empty img");
            }

            if (img.Rows != Height || img.Cols != Width)
            {
                throw new Exception($"Invalid img size: {img.Rows}x{img.Cols}");
            }
            Img = img;
        }

        private static void ResizeIfVaries(Mat mat)
        {
            if (mat.Rows != Width || mat.Cols != Height)
            {
                Cv2.Resize(mat, mat, new Size(Width, Height));
            }
        }

        public static FaceImg CreateColor(Mat mat)
        {
            ResizeIfVaries(mat);
            return new FaceImg(mat);
        }

        public static FaceImg CreateGrayscale(Mat mat)
        {
            ResizeIfVaries(mat);
            Cv2.CvtColor(mat, mat, ColorConversionCodes.BGR2GRAY);
            Cv2.EqualizeHist(mat, mat);
            return new FaceImg(mat);
        }

        public FaceImg Clone()
        {
            return new FaceImg(Img.Clone());
        }

    }
}