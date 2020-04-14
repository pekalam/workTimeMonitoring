using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using FaceRecognitionDotNet;
using Infrastructure.Repositories;
using OpenCvSharp;

namespace Infrastructure.WorkTime
{
    public interface IDnFaceRecognition
    {
        bool CompareFaces(Mat photo1, FaceEncodingData? faceEncoding1, Mat photo2, FaceEncodingData? faceEncoding2);
        FaceEncodingData? GetFaceEncodings(Mat photo);
    }

    public static class SharedFaceRecognitionModel
    {
        private static object _lck = new object();
        private static Task<FaceRecognition> _loadTask;

        static SharedFaceRecognitionModel()
        {
            _loadTask = Task.Factory.StartNew<FaceRecognition>(() => FaceRecognition.Create("."));
        }

        public static List<FaceEncoding> FaceEncodingsSync(Image image,
            IEnumerable<Location> knownFaceLocation = null,
            int numJitters = 1,
            PredictorModel model = PredictorModel.Small)
        {
            lock (_lck)
            {
                return _loadTask.Result.FaceEncodings(image, knownFaceLocation, numJitters, model).ToList();
            }
        }

        public static FaceRecognition Model => _loadTask.Result;
    }

    public class DnFaceRecognition : IDnFaceRecognition
    {
        private const double RecognitionThreshold = 0.55;

        private Image LoadImage(Mat photo)
        {
            var bytes = new byte[photo.Rows * photo.Cols * photo.ElemSize()];
            Marshal.Copy(photo.Data, bytes, 0, bytes.Length);

            var img = FaceRecognition.LoadImage(bytes, photo.Rows, photo.Cols, photo.ElemSize());
            return img;
        }

        private FaceEncodingData? InternalGetFaceEncoding(Image img)
        {
            var imgEncodings = SharedFaceRecognitionModel.FaceEncodingsSync(img, new[] { new Location(0, 0, img.Width, img.Height) }, model: PredictorModel.Small);

            if (imgEncodings.Count != 1)
            {
                return null;
            }

            return new FaceEncodingData(imgEncodings.First());
        }

        private double InternalCompareFaces(Mat photo1, FaceEncodingData? faceEncoding1, Mat photo2, FaceEncodingData? faceEncoding2)
        {
            using var img1 = LoadImage(photo1);


            if (faceEncoding1 == null)
            {
                return double.MaxValue;
            }

            using var img2 = LoadImage(photo2);

            if (faceEncoding2 != null)
            {
                var distance = FaceRecognition.FaceDistance(faceEncoding1.Value, faceEncoding2.Value);
                Debug.WriteLine($"faces dist {distance}");
                return distance;
            }
            else
            {
                return double.MaxValue;
            }
        }

        public bool CompareFaces(Mat photo1, FaceEncodingData? faceEncoding1, Mat photo2, FaceEncodingData? faceEncoding2)
        {
            return InternalCompareFaces(photo1, faceEncoding1, photo2, faceEncoding2) < RecognitionThreshold;
        }

        public FaceEncodingData? GetFaceEncodings(Mat photo)
        {
            using var img1 = LoadImage(photo);
            return InternalGetFaceEncoding(img1);
        }
    }
}