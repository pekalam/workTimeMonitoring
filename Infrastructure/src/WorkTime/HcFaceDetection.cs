using System.Linq;
using OpenCvSharp;

namespace Infrastructure.WorkTime
{
    public interface IHcFaceDetection
    {
        (Rect[] rects, Mat[] faceImgs) DetectFrontalFaces(Mat frame);
        (Rect[] rects, Mat[] faceImgs) DetectProfileFaces(Mat frame);
        (Rect[] rects, Mat[] faceImgs) DetectFrontalThenProfileFaces(Mat frame);
    }

    public class HcFaceDetection : IHcFaceDetection
    {
        private readonly CascadeClassifier _frontClassifier;
        private readonly CascadeClassifier _profileClassifier;

        public HcFaceDetection()
        {
            _frontClassifier = new CascadeClassifier("haarcascade_frontalface_default.xml");
            _profileClassifier = new CascadeClassifier("haarcascade_profileface.xml");
        }

        private Mat[] GetFaceImgs(Rect[] faceRects, Mat frame)
        {
            var faceImgs = new Mat[faceRects.Length];
            for (int i = 0; i < faceRects.Length; i++)
            {
                faceImgs[i] = frame.SubMat(faceRects[i]);
            }

            return faceImgs;
        }

        public (Rect[] rects, Mat[] faceImgs) DetectFrontalFaces(Mat frame)
        {
            var faceRects = _frontClassifier.DetectMultiScale(frame);
            var faceImgs = GetFaceImgs(faceRects, frame);
            return (faceRects, faceImgs);
        }

        public (Rect[] rects, Mat[] faceImgs) DetectProfileFaces(Mat frame)
        {
            var faceRects = _profileClassifier.DetectMultiScale(frame);
            var faceImgs = GetFaceImgs(faceRects, frame);
            return (faceRects, faceImgs);
        }


        public (Rect[] rects, Mat[] faceImgs) DetectFrontalThenProfileFaces(Mat frame)
        {
            var faces = DetectFrontalFaces(frame);
            if (faces.rects.Length == 0)
            {
                faces = DetectProfileFaces(frame);
            }

            return faces;
        }
    }
}