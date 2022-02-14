using Domain.Services;
using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;

namespace WMAlghorithm
{
    public interface IHcFaceDetection
    {
        Rect[] DetectFrontalFaces(Mat frame);
        Rect[] DetectProfileFaces(Mat frame);
        Rect[] DetectFrontalThenProfileFaces(Mat frame);
    }

    public class HcFaceDetectionSettings
    {
        public string ProfileModel { get; set; } = "haarcascade_profileface.xml";
        public string FrontalModel { get; set; } = "haarcascade_frontalface_default.xml";
    }

    public class HcFaceDetection : IHcFaceDetection
    {
        private readonly CascadeClassifier _frontClassifier;
        private readonly CascadeClassifier _profileClassifier;

        private readonly Lazy<Rect[]> _singleElementArray =
            new Lazy<Rect[]>(() => new Rect[1], LazyThreadSafetyMode.None);
        private Rect? _previousFaceRect;

        public HcFaceDetection(IConfigurationService config)
        {
            var settings = config.Get<HcFaceDetectionSettings>("faceDetection");
            _frontClassifier = new CascadeClassifier(settings.FrontalModel);
            _profileClassifier = new CascadeClassifier(settings.ProfileModel);
        }

        private IEnumerable<Rect> EnumerateRemoveNotOverlappingWithBiggest(IEnumerable<Rect> rects)
        {
            var biggest = GetBiggest(rects);
            foreach(var rect in rects)
            {
                if (biggest.IntersectsWith(rect))
                {
                    yield return rect;
                }
            }
        }

        private IEnumerable<Rect> RemoveNotOverlappingWithBiggest(IEnumerable<Rect> rects)
        {
            return !rects.Any() ? rects : EnumerateRemoveNotOverlappingWithBiggest(rects);
        }

        private Rect? Stabilize(in Rect biggest)
        {
            if (!_previousFaceRect.HasValue)
            {
                return null;
            }

            var yOverlap = Math.Min(_previousFaceRect.Value.Y + _previousFaceRect.Value.Height, biggest.Y + biggest.Height)
                           - Math.Max(_previousFaceRect.Value.Y, biggest.Y);
            var xOverlap = Math.Min(_previousFaceRect.Value.X + _previousFaceRect.Value.Width, biggest.X + biggest.Width)
                           - Math.Max(_previousFaceRect.Value.X, biggest.X);

            var prevArea = _previousFaceRect.Value.Height * _previousFaceRect.Value.Width;
            var overlapArea = xOverlap * yOverlap;
            var ratio = overlapArea / (float)prevArea;
            if (!(ratio < 0.95f || ratio > 1f))
            {
                return _previousFaceRect.Value;
            }
            // face rect is moving - we can calculate score regarding rect move and when face is not detected for
            // a few consecutive frames then we can assume that previous rect is a valid rect
            return biggest;
        }

        private Rect GetBiggest(IEnumerable<Rect> rects)
        {
            return rects.OrderByDescending(r => r.Width * r.Height).First();
        }

        private IEnumerable<Rect> RemoveRectanglesDistantFromCenter(IEnumerable<Rect> rects, Mat frame)
        {
            var centerPoint = (x: frame.Width / 2, y: frame.Height / 2);
            return rects.Where(r =>
                centerPoint.x - r.X > r.Width * 2 == false &&
                r.X + r.Width - centerPoint.x > r.Width * 2 == false);
        }

        private IEnumerable<Rect> RemoveDistantFromPrevious(IEnumerable<Rect> rects)
        {
            double Euclidean(in Rect r1, in Rect r2)
            {
                return Math.Sqrt(Math.Pow((r1.X + r1.Width/2) - (r2.X + r2.Width/2), 2) + Math.Pow((r1.Y + r1.Height/2) - (r2.Y + r2.Height/2), 2));
            }

            if (!_previousFaceRect.HasValue)
            {
                return rects;
            }

            return rects.Where(r =>
                Euclidean(r, _previousFaceRect.Value) <= _previousFaceRect.Value.Width/2d);
        }

        private Rect[] GetSingleElementArray(in Rect rect)
        {
            _singleElementArray.Value[0] = rect;
            return _singleElementArray.Value;
        }

        private Rect[] FilterFaceRects(Rect[] faceRects, Mat frame)
        {
            var faceRectsEnumerable = RemoveRectanglesDistantFromCenter(faceRects, frame);
            faceRectsEnumerable = RemoveNotOverlappingWithBiggest(faceRectsEnumerable);
            faceRectsEnumerable = RemoveDistantFromPrevious(faceRectsEnumerable);
            if (!faceRectsEnumerable.Any())
            {
                // remove previous to prevent 'sticking' to biggest
                _previousFaceRect = null;
                return Array.Empty<Rect>();
            }

            // get current biggest
            var biggest = GetBiggest(faceRectsEnumerable);
            // try to stabilize biggest rect with previously saved rect
            var stabilized = Stabilize(biggest);
            if (stabilized.HasValue)
            {
                _previousFaceRect = stabilized.Value;
                return GetSingleElementArray(stabilized.Value);
            }

            // otherwise set previous to biggest
            _previousFaceRect = biggest;
            
            return faceRectsEnumerable.ToArray();
        }

        public Rect[] DetectFrontalFaces(Mat frame)
        {
            var faceRects = _frontClassifier.DetectMultiScale(frame);
            return FilterFaceRects(faceRects, frame);
        }

        public Rect[] DetectProfileFaces(Mat frame)
        {
            var faceRects = _profileClassifier.DetectMultiScale(frame);
            return FilterFaceRects(faceRects, frame);
        }


        public Rect[] DetectFrontalThenProfileFaces(Mat frame)
        {
            var faceRects = DetectFrontalFaces(frame);
            if (faceRects.Length == 0)
            {
                faceRects = DetectProfileFaces(frame);
            }

            return faceRects;
        }
    }
}