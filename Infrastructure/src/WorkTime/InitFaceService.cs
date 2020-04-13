using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FaceRecognitionDotNet;
using Infrastructure.Repositories;
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
        private readonly IHeadPositionService _headPositionService;

        private double _progress;

        public InitFaceService(ITestImageRepository testImageRepository, IDnFaceRecognition dnFaceRecognition,
            IHcFaceDetection faceDetection,
            IHeadPositionService headPositionService)
        {
            _testImageRepository = testImageRepository;
            _dnFaceRecognition = dnFaceRecognition;
            _faceDetection = faceDetection;
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
                ProgressPercentage = (int) _progress,
                Stoped = stopped,
            });
        }

        private Task CreateFaceValidationTask(Mat face1, Mat face2, Task<FaceEncodingData?> faceEncoding1,
            Task<FaceEncodingData?> faceEncoding2,
            CancellationToken ct)
        {
            return Task.Factory.StartNew(() =>
            {
                var fe1 = faceEncoding1.GetAwaiter().GetResult();
                var fe2 = faceEncoding2.GetAwaiter().GetResult();

                if (!_dnFaceRecognition.CompareFaces(face1, fe1, face2, fe2))
                {
                    throw new ArgumentException("Invalid faces");
                }

                _progress += 6;
                ReportInitFaceProgress(null);
            }, ct);
        }

        private Task<FaceEncodingData?> CreateFaceEncodingTask(Mat photo)
        {
            return Task.Factory.StartNew<FaceEncodingData?>(() =>
            {
                var encoding = _dnFaceRecognition.GetFaceEncodings(photo);
                if (encoding == null)
                {
                    throw new ArgumentException("Cannot find face encodings");
                }
                _progress += 6;
                ReportInitFaceProgress(null);
                return encoding;
            });
        }

        public async Task<Task> InitFace(IAsyncEnumerator<Mat> camEnumerator, CancellationToken ct)
        {
            bool interrupted = true;

            var testImages = new List<TestImageBuilder>();
            var tasks = new List<Task>();
            var faceEncodings = new List<Task<FaceEncodingData?>>();

            _progress = 0;
            ResetLearned();


            while (await camEnumerator.MoveNextAsync().ConfigureAwait(true))
            {
                var frame = camEnumerator.Current;

                Rect[] faceRects;
                HeadRotation targetRotation;

                if (testImages.Count == 0)
                {
                    faceRects = _faceDetection.DetectFrontalFaces(frame);
                    if (faceRects.Length != 1)
                    {
                        ReportInitFaceProgress(frame, exception: WorkTime.InitFaceProgress.FaceNotDetected);
                        continue;
                    }

                    var (hRot, vRot) = _headPositionService.GetHeadPosition(frame, faceRects.First());
                    targetRotation = hRot;
                    if (vRot != HeadRotation.Front || hRot != HeadRotation.Front)
                    {
                        ReportInitFaceProgress(frame, face: faceRects.First(),
                            exception: WorkTime.InitFaceProgress.FaceNotStraight);
                        continue;
                    }
                }
                else
                {
                    faceRects = _faceDetection.DetectFrontalThenProfileFaces(frame);
                    if (faceRects.Length != 1)
                    {
                        ReportInitFaceProgress(frame, exception: WorkTime.InitFaceProgress.ProfileFaceNotDetected);

                        continue;
                    }

                    var (hRot, vRot) = _headPositionService.GetHeadPosition(frame, faceRects.First());
                    HeadRotation hTarget = testImages.Count == 1 ? HeadRotation.Left : HeadRotation.Right;
                    HeadRotation vInvalid = testImages.Count == 1 ? HeadRotation.Right : HeadRotation.Left;
                    targetRotation = hTarget;
                    if (hRot != hTarget || vRot == vInvalid)
                    {
                        ReportInitFaceProgress(frame, face: faceRects.First(),
                            exception: WorkTime.InitFaceProgress.FaceNotStraight);
                        continue;
                    }
                }

                var faceImg = frame.Clone();
                var testImageBldr = TestImageBuilderFactory.Create();
                testImageBldr.AddImg(faceImg)
                    .AddDateCreated(DateTime.UtcNow)
                    .AddFaceLocation(faceRects.First())
                    .AddHeadRotation(targetRotation)
                    .SetIsReferenceImg(true);
                testImages.Add(testImageBldr);

                faceEncodings.Add(CreateFaceEncodingTask(faceImg));

                _progress += 20.34;

                ReportInitFaceProgress(frame, faceRects.First());

                if (testImages.Count > 1)
                {
                    var face1 = testImages[^2].Img;
                    var face2 = testImages[^1].Img;
                    tasks.Add(CreateFaceValidationTask(face1, face2, faceEncodings[^2], faceEncodings[^1], ct));
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

            tasks.Add(CreateFaceValidationTask(testImages[0].Img, testImages[^1].Img, faceEncodings[0],
                faceEncodings[^1], ct));


            return Task.WhenAll(tasks).ContinueWith((t) =>
            {
                if (t.Exception != null)
                {
                    _progress = 0;
                    ReportInitFaceProgress(null, exception: WorkTime.InitFaceProgress.FaceRecognitionError,
                        stopped: true);
                    ResetLearned();
                }
                else
                {
                    for (int i = 0; i < testImages.Count; i++)
                    {
                        testImages[i].AddFaceEncoding(faceEncodings[i].Result);
                        _testImageRepository.Add(testImages[i].Build());
                    }

                    _progress = 100;
                    ReportInitFaceProgress(null);
                }
            }, ct);
        }

        private void ResetLearned()
        {
            _testImageRepository.Clear();
        }
    }
}