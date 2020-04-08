using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using FaceRecognitionDotNet;
using OpenCvSharp;

namespace Infrastructure.WorkTime
{
    public interface IDnFaceRecognition
    {
        bool RecognizeFace(Mat photo);
        bool CompareFaces(Mat photo1, Mat photo2);
    }

    public class DnFaceRecognition : IDnFaceRecognition
    {
        private readonly TestImageRepository _testImageRepository;

        public DnFaceRecognition(TestImageRepository testImageRepository)
        {
            _testImageRepository = testImageRepository;
        }

        private Image LoadImage(Mat photo)
        {
            var bytes = new byte[photo.Rows * photo.Cols * photo.ElemSize()];
            Marshal.Copy(photo.Data, bytes, 0, bytes.Length);

            var img = FaceRecognition.LoadImage(bytes, photo.Rows, photo.Cols, photo.ElemSize());
            return img;
        }

        public bool RecognizeFace(Mat photo)
        {
            double sum = 0;
            foreach (var testImg in _testImageRepository.GetAll())
            {
                var distance = InternalCompareFaces(photo, testImg.FaceColor);
                sum += distance;
            }

            double std = sum / _testImageRepository.GetAll().Count;
            return std < 50.0d;
        }

        private double InternalCompareFaces(Mat photo1, Mat photo2, IEnumerable<Location> knownFaceLocation = null)
        {
            using var img = LoadImage(photo1);
            FaceRecognition faceRecognition = FaceRecognition.Create(".");

            var imgEncodings = faceRecognition.FaceEncodings(img);

            using var test = LoadImage(photo2);
            var testEncodings = faceRecognition.FaceEncodings(test, knownFaceLocation);

            if (testEncodings.Any())
            {
                var distance = FaceRecognition.FaceDistance(imgEncodings.First(), testEncodings.First());
                return distance;
            }
            else
            {
                return double.MaxValue;
            }
        }

        private double InternalCompareFaces(Mat photo1, FaceImg faceImg)
        {
            return InternalCompareFaces(photo1, faceImg.Img,
                new[] {new Location(0, 0, faceImg.Img.Width, faceImg.Img.Height),});
        }

        public bool CompareFaces(Mat photo1, Mat photo2)
        {
            return InternalCompareFaces(photo1, photo2) < 50.0d;
        }
    }
}