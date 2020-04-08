using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using FaceRecognitionDotNet;
using OpenCvSharp;
using Point = FaceRecognitionDotNet.Point;

namespace Infrastructure.src.WorkTime
{
    public enum HeadPosition
    {
        Front, Left, Right, Unknown
    }

    public class HeadPositionService
    {
        private FaceRecognition _recognition;

        public HeadPositionService()
        {
            _recognition = FaceRecognition.Create(".");
        }

        

        private HeadPosition EstimatePose(IDictionary<FacePart, IEnumerable<Point>> landmarks, Rect face)
        {
            
            var noseTop = landmarks[FacePart.NoseBridge].First();

            Point center = new Point(face.Location.X + face.Width/2, face.Location.Y + face.Height / 2);

            if (Math.Abs(center.Y - noseTop.Y) == 0)
            {
                return HeadPosition.Front;
            }
            double a = (center.X - noseTop.X) / (double)(center.Y - noseTop.Y);
            double tg = Math.Atan(a);

            return HeadPosition.Left;
        }

        public HeadPosition GetHeadPosition(Mat frame, Rect face)
        {
            var bytes = new byte[frame.Rows * frame.Cols * frame.ElemSize()];
            Marshal.Copy(frame.Data, bytes, 0, bytes.Length);
            var img = FaceRecognition.LoadImage(bytes, frame.Rows, frame.Cols, frame.ElemSize());
            var allLandmarks = _recognition.FaceLandmark(img,
                new[] {new Location(face.Left, face.Top, face.Right, face.Bottom),}, PredictorModel.Large);

            var landmarks = allLandmarks.FirstOrDefault();

            if (landmarks == null)
            {
                return HeadPosition.Unknown;
            }

            return EstimatePose(landmarks, face);
        }
    }
}
