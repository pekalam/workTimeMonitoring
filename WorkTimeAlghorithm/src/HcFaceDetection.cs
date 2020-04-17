using System;
using System.Collections.Generic;
using OpenCvSharp;

namespace WorkTimeAlghorithm
{
    public interface IHcFaceDetection
    {
        Rect[] DetectFrontalFaces(Mat frame);
        Rect[] DetectProfileFaces(Mat frame);
        Rect[] DetectFrontalThenProfileFaces(Mat frame);
    }

    public class HcFaceDetection : IHcFaceDetection
    {
        private readonly CascadeClassifier _frontClassifier;
        private readonly CascadeClassifier _profileClassifier;

        private Rect? _prevFirst;

        public HcFaceDetection()
        {
            _frontClassifier = new CascadeClassifier("haarcascade_frontalface_default.xml");
            _profileClassifier = new CascadeClassifier("haarcascade_profileface.xml");
        }

        private Rect[] RemoveOverlapping(Rect[] rects)
        {
            if (rects.Length == 0)
            {
                return rects;
            }

            var newRects = new List<Rect>();
            var first = rects[0];
            newRects.Add(first);
            for (int i = 1; i < rects.Length; i++)
            {

                if (!first.IntersectsWith(rects[i]))
                {
                    newRects.Add(rects[i]);
                }
            }


            return newRects.ToArray();
        }

        private void Stabilize(Rect[] rects)
        {
            if (!_prevFirst.HasValue)
            {
                return;
            }

            // if (_prevFirst.Value.IntersectsWith(rects[0]))
            // {
            //     rects[0] = _prevFirst.Value;
            // }
            //todo settings param 10, percent of scr
            if (Math.Abs(_prevFirst.Value.X - rects[0].X) < 10 && Math.Abs(_prevFirst.Value.Y - rects[0].Y) < 10)
            {
                rects[0] = _prevFirst.Value;
            }
        } 

        public Rect[] DetectFrontalFaces(Mat frame)
        {
            var faceRects = _frontClassifier.DetectMultiScale(frame);
            faceRects = RemoveOverlapping(faceRects);
            if (faceRects.Length > 0)
            {
                Stabilize(faceRects);
                _prevFirst = faceRects[0];
            }
            else
            {
                _prevFirst = null;
            }
            return faceRects;
        }

        public Rect[] DetectProfileFaces(Mat frame)
        {
            var faceRects = _profileClassifier.DetectMultiScale(frame);
            faceRects = RemoveOverlapping(faceRects);
            if (faceRects.Length > 0)
            {
                Stabilize(faceRects);
                _prevFirst = faceRects[0];
            }
            else
            {
                _prevFirst = null;
            }
            return faceRects;
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