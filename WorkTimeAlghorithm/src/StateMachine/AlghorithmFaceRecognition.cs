using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using Accessibility;
using Domain.User;
using OpenCvSharp;
using Serilog;

namespace WorkTimeAlghorithm.StateMachine
{
    public class AlghorithmFaceRecognition
    {
        private readonly IHcFaceDetection _faceDetection;
        private readonly IDnFaceRecognition _faceRecognition;
        private readonly ICaptureService _captureService;
        private readonly ITestImageRepository _testImageRepository;

        public AlghorithmFaceRecognition(IHcFaceDetection faceDetection, IDnFaceRecognition faceRecognition, ICaptureService captureService, ITestImageRepository testImageRepository)
        {
            _faceDetection = faceDetection;
            _faceRecognition = faceRecognition;
            _captureService = captureService;
            _testImageRepository = testImageRepository;
        }

        public Task<(bool faceDetected, bool faceRecognized)> RecognizeFace(User user)
        {
            return Task.Factory.StartNew<(bool faceDetected, bool faceRecognized)>(() =>
            {
                bool faceDetected, faceRecognized;

                using var frame = _captureService.CaptureSingleFrame();
#if DEV_MODE
                Application.Current.Dispatcher.Invoke(() => Cv2.ImShow("frame", frame));
#endif

                var rects = _faceDetection.DetectFrontalThenProfileFaces(frame);
                if (rects.Length == 0)
                {
                    faceDetected = faceRecognized = false;
                }
                else
                {
                    var ph = _testImageRepository.GetMostRecentImages(user, DateTime.UtcNow.AddDays(-20), 1);
#if DEV_MODE
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        Cv2.ImShow("1", ph.First().Img);
                        Cv2.ImShow("2", frame);

                    });
#endif

                    faceRecognized = _faceRecognition.CompareFaces(ph.First().Img, null, frame, null);
                    faceDetected = true;
                }

                Log.Logger.Debug($"Face detected: {faceDetected} Face recognized: {faceRecognized}");

                return (faceDetected, faceRecognized);
            });
        }
    }
}