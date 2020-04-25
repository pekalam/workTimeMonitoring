using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using FaceRecognitionDotNet;
using OpenCvSharp;
using Point = FaceRecognitionDotNet.Point;

namespace WorkTimeAlghorithm
{
    public enum HeadRotation
    {
        Front,
        Left,
        Right,
        Unknown
    }

    public interface IHeadPositionService
    {
        (HeadRotation hRotation, HeadRotation vRotation) GetHeadPosition(Mat frame, Rect face);
    }

    public class HeadPositionServiceSettings
    {
        public double HorizontalPoseThreshold { get; set; } = 0.20;
        public double VerticalPoseThreshold { get; set; } = 10;
    }

    public class HeadPositionService : IHeadPositionService
    {
        

        private readonly HeadPositionServiceSettings _settings;

        public HeadPositionService(HeadPositionServiceSettings settings)
        {
            _settings = settings;
        }

        private HeadRotation EstimateHorizontalPose(IDictionary<FacePart, IEnumerable<Point>> landmarks, Rect face, Mat frame)
        {
            var noseTop = landmarks[FacePart.NoseBridge].First();
            var left = landmarks[FacePart.Chin].First(p => p.X == landmarks[FacePart.Chin].Min(v => v.X));
            var right = landmarks[FacePart.Chin].First(p => p.X == landmarks[FacePart.Chin].Max(v => v.X));

            #region DEBUG
#if DEV_MODE
            Cv2.Ellipse(frame, new RotatedRect(new Point2f(noseTop.X, noseTop.Y), new Size2f(2, 2), 0), Scalar.Yellow);
            Cv2.Ellipse(frame, new RotatedRect(new Point2f(left.X, left.Y), new Size2f(2, 2), 0), Scalar.Violet);
            Cv2.Ellipse(frame, new RotatedRect(new Point2f(right.X, right.Y), new Size2f(2, 2), 0), Scalar.PaleGreen);
#endif

            #endregion


            int dist = ((noseTop.X - left.X) - (right.X - noseTop.X));

            if (Math.Abs(dist) < _settings.HorizontalPoseThreshold * face.Width)
            {
                return HeadRotation.Front;
            }

            return dist > 0 ? HeadRotation.Left : HeadRotation.Right;
        }

        private HeadRotation EstimateVerticalPose(IDictionary<FacePart, IEnumerable<Point>> landmarks, Rect face, Mat frame)
        {
            var leftEye = landmarks[FacePart.LeftEyebrow].First();
            var rightEye = landmarks[FacePart.RightEyebrow].Last();

            #region DEBUG
#if DEV_MODE
            Cv2.Ellipse(frame, new RotatedRect(new Point2f(leftEye.X, leftEye.Y), new Size2f(2, 2), 0), Scalar.Red);
            Cv2.Ellipse(frame, new RotatedRect(new Point2f(rightEye.X, rightEye.Y), new Size2f(2, 2), 0), Scalar.Red);
#endif
            #endregion

            if (leftEye.X - rightEye.X == 0)
            {
                throw new Exception("Cyclope exception");
            }
            double a = (leftEye.Y - rightEye.Y) / (double)(leftEye.X - rightEye.X);
            double angle = Math.Atan(a) * 180.0 / Math.PI;

            //todo to rad
            if (Math.Abs(angle) < _settings.VerticalPoseThreshold)
            {
                return HeadRotation.Front;
            }

            return angle > 0 ? HeadRotation.Left : HeadRotation.Right;
        }

        public (HeadRotation hRotation, HeadRotation vRotation) GetHeadPosition(Mat frame, Rect face)
        {
            var bytes = new byte[frame.Rows * frame.Cols * frame.ElemSize()];
            Marshal.Copy(frame.Data, bytes, 0, bytes.Length);
            using var img = FaceRecognition.LoadImage(bytes, frame.Rows, frame.Cols, frame.ElemSize());
            var allLandmarks = SharedFaceRecognitionModel.Model.FaceLandmark(img,
                new[] {new Location(face.Left, face.Top, face.Right, face.Bottom),}, PredictorModel.Large);

            var landmarks = allLandmarks.FirstOrDefault();

            if (landmarks == null)
            {
                return (HeadRotation.Unknown, HeadRotation.Unknown);
            }

            #region DEBUG
#if DEV_MODE
            foreach (var facePart in Enum.GetValues(typeof(FacePart)).Cast<FacePart>())
            {
                foreach (var landmark in landmarks)
                {
                    foreach (var p in landmark.Value.ToArray()) 
                        Cv2.Ellipse(frame, new RotatedRect(new Point2f(p.X, p.Y), new Size2f(2, 2), 0), Scalar.Aqua);
                }
            }
#endif
            #endregion

            return (EstimateHorizontalPose(landmarks, face, frame), EstimateVerticalPose(landmarks, face, frame));
        }
    }
}