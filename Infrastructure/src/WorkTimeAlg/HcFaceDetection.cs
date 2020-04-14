using OpenCvSharp;

namespace Infrastructure.WorkTimeAlg
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

        public HcFaceDetection()
        {
            _frontClassifier = new CascadeClassifier("haarcascade_frontalface_default.xml");
            _profileClassifier = new CascadeClassifier("haarcascade_profileface.xml");
        }

        public Rect[] DetectFrontalFaces(Mat frame)
        {
            var faceRects = _frontClassifier.DetectMultiScale(frame);
            return faceRects;
        }

        public Rect[] DetectProfileFaces(Mat frame)
        {
            var faceRects = _profileClassifier.DetectMultiScale(frame);
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