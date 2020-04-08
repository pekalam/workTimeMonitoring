using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Infrastructure.src.WorkTime;
using OpenCvSharp;

namespace Infrastructure.WorkTime
{
    public class InitFaceProgressArgs
    {
        public Mat Frame { get; set; }
        public Rect? FaceRect { get; set; }
        public InitFaceProgress ProgressState { get; set; }
        public int ProgressPercentage { get; set; }
        public bool Stoped { get; set; }
    }

    public enum InitFaceProgress
    {
        Progress,
        InvalidFacePos,
        FaceNotDetected,
        FaceRecognitionError,
        ProfileFaceNotDetected,
        FaceNotStraight,
    }

    public class InitFaceService
    {
        private readonly ITestImageRepository _testImageRepository;
        private readonly IDnFaceRecognition _dnFaceRecognition;
        private readonly IHcFaceDetection _faceDetection;
        private readonly ILbphFaceRecognition _lbphFaceRecognition;
        private readonly ICaptureService _captureService;
        private readonly IHeadPositionService _headPositionService;

        private int _progress;

        public InitFaceService(ITestImageRepository testImageRepository, IDnFaceRecognition dnFaceRecognition,
            IHcFaceDetection faceDetection, ILbphFaceRecognition lbphFaceRecognition, ICaptureService captureService,
            IHeadPositionService headPositionService)
        {
            _testImageRepository = testImageRepository;
            _dnFaceRecognition = dnFaceRecognition;
            _faceDetection = faceDetection;
            _lbphFaceRecognition = lbphFaceRecognition;
            _captureService = captureService;
            _headPositionService = headPositionService;
        }

        public IProgress<InitFaceProgressArgs> InitFaceProgress { get; set; }

        private void ReportInitFaceProgress(Mat img, Rect? face = null,
            InitFaceProgress exception = WorkTime.InitFaceProgress.Progress,
            bool stopped = false)
        {
            InitFaceProgress?.Report(new InitFaceProgressArgs()
            {
                ProgressState = exception,
                FaceRect = face,
                Frame = img,
                ProgressPercentage = _progress,
                Stoped = stopped,
            });
        }

        private Task CreateFaceValidationTask(FaceImg face1, FaceImg face2, CancellationToken ct)
        {
            return Task.Factory.StartNew(() =>
            {
                if (!_dnFaceRecognition.CompareFaces(face1.Img, face2.Img))
                {
                    throw new ArgumentException("Invalid faces");
                }

                _progress += 25 / 3;
                if (_progress == 99)
                {
                    _progress = 100;
                }

                ReportInitFaceProgress(null);
            }, ct);
        }

        public async Task InitFace(CancellationTokenSource cts)
        {
            var capCts =
                CancellationTokenSource.CreateLinkedTokenSource(new CancellationTokenSource().Token, cts.Token);
            var vcts = CancellationTokenSource.CreateLinkedTokenSource(new CancellationTokenSource().Token, cts.Token);
            bool interrupted = true;

            var toCapture = new List<TestImage>();
            var validationTasks = new List<Task>();

            _progress = 0;
            ResetLearned();

            await foreach (var frame in _captureService.CaptureFrames(capCts.Token).ConfigureAwait(false))
            {
                Rect[] faceRects;
                Mat[] faces;

                if (toCapture.Count == 0)
                {
                    (faceRects, faces) = _faceDetection.DetectFrontalFaces(frame);
                    if (faces.Length != 1)
                    {
                        ReportInitFaceProgress(frame, exception: WorkTime.InitFaceProgress.FaceNotDetected);
                        continue;
                    }

                    var (hRot, vRot) = _headPositionService.GetHeadPosition(frame, faceRects.First());
                    if (vRot != HeadRotation.Front || hRot != HeadRotation.Front)
                    {
                        ReportInitFaceProgress(frame, exception: WorkTime.InitFaceProgress.FaceNotStraight);
                        continue;
                    }
                }
                else
                {
                    (faceRects, faces) = _faceDetection.DetectFrontalThenProfileFaces(frame);
                    if (faces.Length != 1)
                    {
                        ReportInitFaceProgress(frame, exception: WorkTime.InitFaceProgress.ProfileFaceNotDetected);

                        continue;
                    }
                    var (hRot, vRot) = _headPositionService.GetHeadPosition(frame, faceRects.First());
                    HeadRotation hTarget = toCapture.Count == 1 ? HeadRotation.Left : HeadRotation.Right;
                    HeadRotation vInvalid = toCapture.Count == 1 ? HeadRotation.Right : HeadRotation.Left;
                    if (hRot != hTarget || vRot == vInvalid)
                    {
                        ReportInitFaceProgress(frame, exception: WorkTime.InitFaceProgress.FaceNotStraight);
                        continue;
                    }
                }

                var testImg = TestImage.CreateFromFace(faces.First());
                toCapture.Add(testImg);

                _progress += 20;

                ReportInitFaceProgress(frame, faceRects.First());

                if (toCapture.Count > 1)
                {
                    var face1 = toCapture[^2];
                    var face2 = toCapture[^1];
                    validationTasks.Add(CreateFaceValidationTask(face1.FaceColor, face2.FaceColor, vcts.Token));
                }

                if (toCapture.Count == 3)
                {
                    interrupted = false;
                    capCts.Cancel();
                }
            }

            if (toCapture.Count != 3 || interrupted)
            {
                ResetLearned();
                throw new Exception();
            }

            validationTasks.Add(CreateFaceValidationTask(toCapture[0].FaceColor, toCapture[^1].FaceColor, vcts.Token));

            foreach (var testImage in toCapture)
            {
                try
                {
                    _lbphFaceRecognition.Train(testImage.FaceGrayscale);
                    _testImageRepository.Add(testImage);
                    _progress += 5;
                    ReportInitFaceProgress(null);
                }
                catch (Exception e)
                {
                    vcts.Cancel();
                    _progress = 0;
                    ReportInitFaceProgress(null, exception: WorkTime.InitFaceProgress.FaceRecognitionError,
                        stopped: true);
                    _testImageRepository.Clear();
                    return;
                }
            }


            try
            {
                await Task.WhenAll(validationTasks);
            }
            catch (Exception e)
            {
                _progress = 0;
                ReportInitFaceProgress(null, exception: WorkTime.InitFaceProgress.FaceRecognitionError, stopped: true);
                ResetLearned();
                return;
            }
        }

        private void ResetLearned()
        {
            _testImageRepository.Clear();
            _lbphFaceRecognition.Reset();
        }
    }
}