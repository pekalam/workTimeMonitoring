using Domain.User;
using OpenCvSharp;
using Serilog;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using Rect = OpenCvSharp.Rect;

namespace WMAlghorithm.StateMachine
{
    public class AlghorithmFaceRecognition
    {
        private readonly IHcFaceDetection _faceDetection;
        private readonly IDnFaceRecognition _faceRecognition;
        private readonly ICaptureService _captureService;
        private readonly ITestImageRepository _testImageRepository;

        public AlghorithmFaceRecognition(IHcFaceDetection faceDetection, IDnFaceRecognition faceRecognition,
            ICaptureService captureService, ITestImageRepository testImageRepository)
        {
            _faceDetection = faceDetection;
            _faceRecognition = faceRecognition;
            _captureService = captureService;
            _testImageRepository = testImageRepository;
        }

        private Mat[] ExcludeFaces(Mat frame, Rect[] faces)
        {
            if (faces.Length == 1)
            {
                return new[] {frame};
            }
            else
            {
                var ret = new Mat[faces.Length];
                for (int i = 0; i < faces.Length; i++)
                {
                    var newFrame = frame.Clone();
                    for (int j = 0; j < faces.Length; j++)
                    {
                        if (j != i)
                        {
                            Cv2.Rectangle(newFrame, faces[j], Scalar.Black, -1);
                        }
                    }

                    ret[i] = newFrame;
                }

                return ret;
            }
        }

        private (bool faceDetected, bool faceRecognized) CompareFaces(Mat frame, User user, Rect[] rects)
        {
            bool faceDetected = false, faceRecognized = false;

            if (rects.Length > 0)
            {
                var ph = _testImageRepository.GetReferenceImages(user);

                #region DEV_MODE

#if DEV_MODE
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        Cv2.ImShow("1", ph.First().Img);
                        Cv2.ImShow("2", frame);
                    });
#endif

                #endregion

                var excluded = ExcludeFaces(frame, rects);

                foreach (var mat in excluded)
                {
                    faceRecognized = _faceRecognition.CompareFaces(
                        ph.First(i => i.HorizontalHeadRotation == HeadRotation.Front).Img, null, frame, null);

                    if (faceRecognized)
                    {
                        break;
                    }

                    faceRecognized = _faceRecognition.CompareFaces(
                        ph.First(i => i.HorizontalHeadRotation == HeadRotation.Right).Img, null, frame, null);

                    if (faceRecognized)
                    {
                        break;
                    }

                    faceRecognized = _faceRecognition.CompareFaces(
                        ph.First(i => i.HorizontalHeadRotation == HeadRotation.Left).Img, null, frame, null);
                }

                faceDetected = true;
            }

            Log.Logger.Debug($"Face detected: {faceDetected} Face recognized: {faceRecognized}");

            return (faceDetected, faceRecognized);
        }

        public Task<(bool faceDetected, bool faceRecognized)> RecognizeFace(User user, Mat frame) =>
            Task.Factory.StartNew(() =>
            {
                #region DEV_MODE

#if DEV_MODE
                Application.Current.Dispatcher.Invoke(() => Cv2.ImShow("frame", frame));
#endif

                #endregion

                var rects = _faceDetection.DetectFrontalThenProfileFaces(frame);
                var (faceDetected, faceRecognized) = CompareFaces(frame, user, rects);
                return (faceDetected, faceRecognized);
            });

        public Task<(bool faceDetected, bool faceRecognized)> RecognizeFace(User user) =>
            Task.Factory.StartNew(() =>
            {
                using var frame = _captureService.CaptureSingleFrame();
                var rects = _faceDetection.DetectFrontalThenProfileFaces(frame);
                return CompareFaces(frame, user, rects);
            });
    }
}