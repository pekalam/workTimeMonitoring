using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using FaceRecognitionDotNet;
using OpenCvSharp;

namespace Infrastructure.WorkTime
{
    public interface IDnFaceRecognition
    {
        bool RecognizeFace(Mat photo);
        bool CompareFaces(Mat photo1, Mat photo2, Rect? face1 = null, Rect? face2 = null);
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

        private double InternalCompareFaces(Mat photo1, Mat photo2, Location? knownFaceLocation1 = null,
            Location? knownFaceLocation2 = null)
        {
            using var img = LoadImage(photo1);
            FaceRecognition faceRecognition = FaceRecognition.Create(".");

            var imgEncodings = faceRecognition.FaceEncodings(img, new []{knownFaceLocation1}, model: PredictorModel.Large);

            if (!imgEncodings.Any())
            {
                return double.MaxValue;
            }

            using var test = LoadImage(photo2);
            var testEncodings = faceRecognition.FaceEncodings(test, new []{knownFaceLocation2}, model: PredictorModel.Large);

            if (testEncodings.Any())
            {
                var distance = FaceRecognition.FaceDistance(imgEncodings.First(), testEncodings.First());
                Debug.WriteLine($"faces dist {distance}");
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
                knownFaceLocation2: new Location(0, 0, faceImg.Img.Width, faceImg.Img.Height));
        }

        private Location? RectToLocation(Rect? rect)
        {
            if (!rect.HasValue)
            {
                return null;
            }
            return new Location(rect.Value.Left, rect.Value.Top, rect.Value.Right, rect.Value.Bottom);
        }

        public bool CompareFaces(Mat photo1, Mat photo2, Rect? face1 = null, Rect? face2 = null)
        {
            return InternalCompareFaces(photo1, photo2, knownFaceLocation1: RectToLocation(face1),
                knownFaceLocation2: RectToLocation(face2)) < 50.0d;
        }
    }
}