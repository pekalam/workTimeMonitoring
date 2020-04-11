using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using OpenCvSharp;

namespace Infrastructure.WorkTime
{
    public class InitFaceProgressArgs
    {
        public Mat? Frame { get; set; }
        public Rect? FaceRect { get; set; }
        public InitFaceProgress ProgressState { get; set; }
        public int ProgressPercentage { get; set; }
        public bool Stoped { get; set; }
    }

    public enum InitFaceProgress
    {
        Progress,
        FaceNotDetected,
        FaceRecognitionError,
        ProfileFaceNotDetected,
        FaceNotStraight,
        CancelledByUser,
    }

    public class InitFaceService
    {
        private readonly ITestImageRepository _testImageRepository;
        private readonly IDnFaceRecognition _dnFaceRecognition;
        private readonly IHcFaceDetection _faceDetection;
        private readonly ILbphFaceRecognition _lbphFaceRecognition;
        private readonly IHeadPositionService _headPositionService;

        private double _progress;

        public InitFaceService(ITestImageRepository testImageRepository, IDnFaceRecognition dnFaceRecognition,
            IHcFaceDetection faceDetection, ILbphFaceRecognition lbphFaceRecognition,
            IHeadPositionService headPositionService)
        {
            _testImageRepository = testImageRepository;
            _dnFaceRecognition = dnFaceRecognition;
            _faceDetection = faceDetection;
            _lbphFaceRecognition = lbphFaceRecognition;
            _headPositionService = headPositionService;
        }

        public IProgress<InitFaceProgressArgs>? InitFaceProgress { get; set; }

        private void ReportInitFaceProgress(Mat? img, Rect? face = null,
            InitFaceProgress exception = WorkTime.InitFaceProgress.Progress,
            bool stopped = false)
        {
            InitFaceProgress?.Report(new InitFaceProgressArgs()
            {
                ProgressState = exception,
                FaceRect = face,
                Frame = img,
                ProgressPercentage = (int)_progress,
                Stoped = stopped,
            });
        }

        private Task CreateFaceValidationTask(Mat face1, Rect face1Location, Mat face2, Rect face2Location,
            CancellationToken ct)
        {
            return Task.Factory.StartNew(() =>
            {
                if (!_dnFaceRecognition.CompareFaces(face1, face2, face1Location, face2Location))
                {
                    throw new ArgumentException("Invalid faces");
                }

                _progress += 6;
                ReportInitFaceProgress(null);
            }, ct);
        }

        private Task CreateLpbhTrainingTask(List<TestImage> toCapture, CancellationToken ct)
        {
            return Task.Factory.StartNew(() =>
            {
                foreach (var testImage in toCapture)
                {
                    _lbphFaceRecognition.Train(testImage.FaceGrayscale);
                    _testImageRepository.Add(testImage);
                    _progress += 6;
                    ReportInitFaceProgress(null);
                }
            }, ct);
        }

        public async Task<Task> InitFace(IAsyncEnumerator<Mat> camEnumerator, CancellationToken ct)
        {
            bool interrupted = true;

            var testImages = new List<TestImage>();
            var faceLocations = new List<Rect>();
            var capturedFrames = new List<Mat>();
            var tasks = new List<Task>();

            _progress = 0;
            ResetLearned();
            while (await camEnumerator.MoveNextAsync().ConfigureAwait(true))
            {
                var frame = camEnumerator.Current;
                Rect[] faceRects;
                Mat[] faces;

                if (testImages.Count == 0)
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
                        ReportInitFaceProgress(frame, face: faceRects.First(),
                            exception: WorkTime.InitFaceProgress.FaceNotStraight);
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
                    HeadRotation hTarget = testImages.Count == 1 ? HeadRotation.Left : HeadRotation.Right;
                    HeadRotation vInvalid = testImages.Count == 1 ? HeadRotation.Right : HeadRotation.Left;
                    if (hRot != hTarget || vRot == vInvalid)
                    {
                        ReportInitFaceProgress(frame, face: faceRects.First(),
                            exception: WorkTime.InitFaceProgress.FaceNotStraight);
                        continue;
                    }
                }

                var testImg = TestImage.CreateFromFace(faces.First());
                testImages.Add(testImg);
                faceLocations.Add(faceRects.First());
                capturedFrames.Add(frame.Clone());

                _progress += 21.34;

                ReportInitFaceProgress(frame, faceRects.First());

                if (testImages.Count > 1)
                {
                    var face1 = capturedFrames[^2];
                    var face2 = capturedFrames[^1];
                    tasks.Add(CreateFaceValidationTask(face1, faceLocations[^2], face2,
                        faceLocations[^1], ct));
                }

                if (testImages.Count == 3)
                {
                    interrupted = false;
                    break;
                }
            }

            if (interrupted)
            {
                ResetLearned();
                ReportInitFaceProgress(null, exception: WorkTime.InitFaceProgress.CancelledByUser, stopped: true);
                return Task.CompletedTask;
            }

            tasks.Add(CreateFaceValidationTask(capturedFrames[0], faceLocations[0], capturedFrames[^1],
                faceLocations[^1], ct));

            tasks.Add(CreateLpbhTrainingTask(testImages, ct));


            return Task.WhenAll(tasks).ContinueWith((t) =>
            {
                if (t.Exception != null)
                {
                    _progress = 0;
                    ReportInitFaceProgress(null, exception: WorkTime.InitFaceProgress.FaceRecognitionError,
                        stopped: true);
                    ResetLearned();
                }
            }, ct);
        }

        private void ResetLearned()
        {
            _testImageRepository.Clear();
            _lbphFaceRecognition.Reset();
        }
    }
}