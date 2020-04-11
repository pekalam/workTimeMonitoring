using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Infrastructure.WorkTime;
using Moq;
using Moq.AutoMock;
using OpenCvSharp;
using Xunit;

namespace UnitTests
{
    static class TestUtilsHelpers
    {
        public static int AsInt(this string str)
        {
            return Convert.ToInt32(str);
        }
    }

    static class HcFaceDetectionTestHelpers
    {
        public static void ReturnFace(this Mock<IHcFaceDetection> mHcFaceDetection, ImageTestUtils testUtils,
            string imgName)
        {
            mHcFaceDetection.Setup(f => f.DetectFrontalFaces(It.IsAny<Mat>())).Returns(() =>
            {
                var ret = testUtils.GetFaceImg(imgName);
                return (new[] {ret.faceRect}, new[] {ret.faceImg});
            });
        }

        public static void ReturnFrontProfileFace(this Mock<IHcFaceDetection> mHcFaceDetection,
            ImageTestUtils testUtils, string imgName)
        {
            mHcFaceDetection.Setup(f => f.DetectFrontalThenProfileFaces(It.IsAny<Mat>())).Returns(() =>
            {
                var ret = testUtils.GetFaceImg(imgName);
                return (new[] {ret.faceRect}, new[] {ret.faceImg});
            });
        }
    }

    class ImageTestUtils
    {
        public ImageTestUtils()
        {
            TestImgDir = Environment.GetEnvironmentVariable(nameof(TestImgDir));
            if (!Directory.Exists(TestImgDir))
            {
                throw new Exception($"Directory {TestImgDir} doesn't exist");
            }
        }

        public string TestImgDir { get; }

        public (Rect faceRect, Mat faceImg) GetFaceImg(string imgName)
        {
            var fileName = Directory.EnumerateFiles(TestImgDir).FirstOrDefault(n => n.Contains(imgName));
            if (fileName == null)
            {
                throw new Exception($"Invalid imgName: {imgName}");
            }

            var parts = fileName.Split('_', StringSplitOptions.RemoveEmptyEntries);
            parts[^1] = parts[^1].Split('.', StringSplitOptions.RemoveEmptyEntries).First();

            if (parts.Length != 5)
            {
                throw new Exception($"Invalid filename: {fileName}");
            }

            var rect = new Rect(parts[1].AsInt(), parts[2].AsInt(), parts[3].AsInt(), parts[4].AsInt());
            var faceImg = Cv2.ImRead(fileName);

            return (rect, faceImg);
        }

        public TestImage GetTestImage(string imgName)
        {
            var img = GetFaceImg(imgName);
            var testImg = TestImage.CreateFromFace(img.faceImg);
            return testImg;
        }

        public Mat GetImage(string imgName)
        {
            var fileName = Directory.EnumerateFiles(TestImgDir).FirstOrDefault(n => n.Contains(imgName));
            if (fileName == null)
            {
                throw new Exception($"Invalid imgName: {imgName}");
            }

            return Cv2.ImRead(fileName);
        }
    }

    public class InitFaceServiceTests
    {
        private ImageTestUtils _testUtils = new ImageTestUtils();
        private AutoMocker _mocker = new AutoMocker();
        private readonly Mat _nonEmptyFrame = Mat.Zeros(600, 800, MatType.CV_32FC3);

        private Mock<IHcFaceDetection> SetupHcFaceDetection()
        {
            var mFaceDetection = new Mock<IHcFaceDetection>();
            _mocker.Use<IHcFaceDetection>(mFaceDetection.Object);
            return mFaceDetection;
        }

        private Mock<ITestImageRepository> SetupTestImageRepository()
        {
            var mTestImageRepository = new Mock<ITestImageRepository>();
            _mocker.Use<ITestImageRepository>(mTestImageRepository.Object);
            return mTestImageRepository;
        }


        private Mock<IDnFaceRecognition> SetupDnFaceRecognition()
        {
            var mDnFaceRecognition = new Mock<IDnFaceRecognition>();
            _mocker.Use<IDnFaceRecognition>(mDnFaceRecognition.Object);
            return mDnFaceRecognition;
        }

        private Mock<ILbphFaceRecognition> SetupLbphFaceRecognition()
        {
            var mLbphFaceRecognition = new Mock<ILbphFaceRecognition>();
            _mocker.Use<ILbphFaceRecognition>(mLbphFaceRecognition.Object);
            return mLbphFaceRecognition;
        }

        private Mock<ICaptureService> SetupCaptureService()
        {
            var mCaptureService = new Mock<ICaptureService>();
            _mocker.Use<ICaptureService>(mCaptureService.Object);
            return mCaptureService;
        }

        private Mock<IHeadPositionService> SetupHeadPositionService()
        {
            var mHeadPositionService = new Mock<IHeadPositionService>();
            _mocker.Use<IHeadPositionService>(mHeadPositionService.Object);
            return mHeadPositionService;
        }

        private Expression<Func<InitFaceProgressArgs, bool>> FaceNotDetected()
        {
            return (args) => args.Frame == _nonEmptyFrame &&
                             args.ProgressState == Infrastructure.WorkTime.InitFaceProgress.FaceNotDetected &&
                             !args.Stoped;
        }

        private Expression<Func<InitFaceProgressArgs, bool>> ProfileFaceNotDetected()
        {
            return (args) => args.Frame == _nonEmptyFrame &&
                             args.ProgressState == Infrastructure.WorkTime.InitFaceProgress.ProfileFaceNotDetected &&
                             !args.Stoped;
        }

        private Expression<Func<InitFaceProgressArgs, bool>> InitFaceProgress()
        {
            return (args) => args.Frame == _nonEmptyFrame &&
                             args.ProgressPercentage > 0 &&
                             args.ProgressState == Infrastructure.WorkTime.InitFaceProgress.Progress &&
                             !args.Stoped;
        }

        private Expression<Func<InitFaceProgressArgs, bool>> InitFaceProgress(Mat frame, int progress)
        {
            return (args) => args.Frame == frame &&
                             args.ProgressPercentage == progress &&
                             args.ProgressState == Infrastructure.WorkTime.InitFaceProgress.Progress &&
                             !args.Stoped;
        }

        private Expression<Func<InitFaceProgressArgs, bool>> FaceNotStraight()
        {
            return (args) => args.Frame == _nonEmptyFrame &&
                             args.ProgressState == Infrastructure.WorkTime.InitFaceProgress.FaceNotStraight &&
                             !args.Stoped;
        }

        private Expression<Func<InitFaceProgressArgs, bool>> CancelledByUser()
        {
            return (args) =>
                args.Frame == null && args.ProgressState == Infrastructure.WorkTime.InitFaceProgress.CancelledByUser
                                   && args.Stoped;
        }

        private Expression<Func<InitFaceProgressArgs, bool>> FaceRecognitionError()
        {
            return (args) =>
                args.Frame == null && args.ProgressState == Infrastructure.WorkTime.InitFaceProgress.FaceRecognitionError
                                   && args.Stoped;
        }

        //
        // [Fact]
        // public async Task InitFace_when_faces_not_captured_and_interrupted_throws()
        // {
        //     var mProgress = new Mock<IProgress<InitFaceProgressArgs>>();
        //     var mHcFaceDetection = SetupHcFaceDetection();
        //     var mTestImageRepository = SetupTestImageRepository();
        //     var mLbphFaceRecognition = SetupLbphFaceRecognition();
        //     var mCaptureService = SetupCaptureService();
        //     var mDnFaceRecognition = SetupDnFaceRecognition();
        //     SetupHeadPositionService();
        //
        //     async IAsyncEnumerable<Mat> InterruptedCapture()
        //     {
        //         mHcFaceDetection.Setup(f => f.DetectFrontalFaces(It.IsAny<Mat>())).Returns((new Rect[0], new Mat[0]));
        //         yield return _nonEmptyFrame;
        //         yield return _nonEmptyFrame;
        //     }
        //
        //     mCaptureService.Setup(f => f.CaptureFrames(It.IsAny<CancellationToken>())).Returns(InterruptedCapture);
        //     mTestImageRepository.SetupGet(f => f.Count).Returns(0);
        //
        //     var initTestService = _mocker.CreateInstance<InitFaceService>();
        //
        //     initTestService.InitFaceProgress = mProgress.Object;
        //
        //     await Assert.ThrowsAnyAsync<Exception>(async () =>
        //         await initTestService.InitFace(mCaptureService.Object.CaptureFrames(CancellationToken.None).GetAsyncEnumerator(),
        //             CancellationToken.None));
        //
        //     mTestImageRepository.Verify(f => f.Clear(), Times.Exactly(2));
        //     mLbphFaceRecognition.Verify(f => f.Reset(), Times.Exactly(2));
        // }


        [Fact]
        public async Task InitFace_when_faces_not_captured_and_interrupted_returns_cancelled_progress()
        {
            var mProgress = new Mock<IProgress<InitFaceProgressArgs>>();
            var mHcFaceDetection = SetupHcFaceDetection();
            var mTestImageRepository = SetupTestImageRepository();
            var mLbphFaceRecognition = SetupLbphFaceRecognition();
            var mCaptureService = SetupCaptureService();
            var mDnFaceRecognition = SetupDnFaceRecognition();
            SetupHeadPositionService();

            async IAsyncEnumerable<Mat> InterruptedCapture()
            {
                mHcFaceDetection.Setup(f => f.DetectFrontalFaces(It.IsAny<Mat>())).Returns((new Rect[0], new Mat[0]));
                yield return _nonEmptyFrame;
                yield return _nonEmptyFrame;
            }

            var cts = new CancellationTokenSource();

            mCaptureService.Setup(f => f.CaptureFrames(cts.Token)).Returns(InterruptedCapture);
            mTestImageRepository.SetupGet(f => f.Count).Returns(0);

            var initTestService = _mocker.CreateInstance<InitFaceService>();
            initTestService.InitFaceProgress = mProgress.Object;

            cts.Cancel();
            await initTestService.InitFace(
                mCaptureService.Object.CaptureFrames(cts.Token).GetAsyncEnumerator(cts.Token),
                CancellationToken.None);

            mProgress.Verify(f => f.Report(It.Is(CancelledByUser())), Times.Once());

            mTestImageRepository.Verify(f => f.Clear(), Times.Exactly(2));
            mLbphFaceRecognition.Verify(f => f.Reset(), Times.Exactly(2));
        }

        [Fact]
        public async Task InitFace_when_dn_compare_faces_throws_returns_error_progress()
        {
            var mProgress = new Mock<IProgress<InitFaceProgressArgs>>();
            var mHcFaceDetection = SetupHcFaceDetection();
            var mTestImageRepository = SetupTestImageRepository();
            var mLbphFaceRecognition = SetupLbphFaceRecognition();
            var mCaptureService = SetupCaptureService();
            var mDnFaceRecognition = SetupDnFaceRecognition();
            var mHeadPositionService = SetupHeadPositionService();

            mDnFaceRecognition
                .Setup(f => f.CompareFaces(It.IsAny<Mat>(), It.IsAny<Mat>(), It.IsAny<Rect?>(), It.IsAny<Rect?>()))
                .Returns(false);
            mCaptureService.Setup(f => f.CaptureFrames(It.IsAny<CancellationToken>())).Returns(SuccessfulFaceCapturingScenario(mHcFaceDetection, mProgress, mHeadPositionService));
            var initTestService = _mocker.CreateInstance<InitFaceService>();
            initTestService.InitFaceProgress = mProgress.Object; 
            await await initTestService.InitFace(mCaptureService.Object.CaptureFrames(CancellationToken.None).GetAsyncEnumerator(),
                CancellationToken.None);

            mProgress.Verify(f => f.Report(It.Is(InitFaceProgress(_nonEmptyFrame, 100))), Times.Never());
            mProgress.Verify(f => f.Report(It.Is(InitFaceProgress(null, 100))), Times.Never());
            mProgress.Verify(f => f.Report(It.Is(FaceRecognitionError())), Times.Once());

            mTestImageRepository.Verify(f => f.Clear(), Times.Exactly(2));
            mLbphFaceRecognition.Verify(f => f.Reset(), Times.Exactly(2));
        }

        [Fact]
        public async Task InitFace_when_lpbh_train_throws_returns_error_progress()
        {
            var mProgress = new Mock<IProgress<InitFaceProgressArgs>>();
            var mHcFaceDetection = SetupHcFaceDetection();
            var mTestImageRepository = SetupTestImageRepository();
            var mLbphFaceRecognition = SetupLbphFaceRecognition();
            var mCaptureService = SetupCaptureService();
            var mDnFaceRecognition = SetupDnFaceRecognition();
            var mHeadPositionService = SetupHeadPositionService();

            mLbphFaceRecognition.Setup(f => f.Train(It.IsAny<FaceImg>())).Throws(new Exception());
            mCaptureService.Setup(f => f.CaptureFrames(It.IsAny<CancellationToken>())).Returns(SuccessfulFaceCapturingScenario(mHcFaceDetection, mProgress, mHeadPositionService));
            var initTestService = _mocker.CreateInstance<InitFaceService>();
            initTestService.InitFaceProgress = mProgress.Object;
            await await initTestService.InitFace(mCaptureService.Object.CaptureFrames(CancellationToken.None).GetAsyncEnumerator(),
                CancellationToken.None);

            mProgress.Verify(f => f.Report(It.Is(InitFaceProgress(_nonEmptyFrame, 100))), Times.Never());
            mProgress.Verify(f => f.Report(It.Is(InitFaceProgress(null, 100))), Times.Never());
            mProgress.Verify(f => f.Report(It.Is(FaceRecognitionError())), Times.Once());

            mTestImageRepository.Verify(f => f.Clear(), Times.Exactly(2));
            mLbphFaceRecognition.Verify(f => f.Reset(), Times.Exactly(2));
        }


        [Fact]
        public async Task InitFace_when_invalid_frames_returns_valid_progress_states()
        {
            var mProgress = new Mock<IProgress<InitFaceProgressArgs>>();
            var mHcFaceDetection = SetupHcFaceDetection();
            var mTestImageRepository = SetupTestImageRepository();
            var mLbphFaceRecognition = SetupLbphFaceRecognition();
            var mCaptureService = SetupCaptureService();
            var mDnFaceRecognition = SetupDnFaceRecognition();
            var mHeadPositionService = SetupHeadPositionService();

            async IAsyncEnumerable<Mat> InvalidCapture()
            {
                mHcFaceDetection.Setup(f => f.DetectFrontalFaces(It.IsAny<Mat>())).Returns((new Rect[0], new Mat[0]));
                yield return _nonEmptyFrame;
                mProgress.Verify(f => f.Report(It.Is(FaceNotDetected())), Times.Once());

                mHeadPositionService.Setup(f => f.GetHeadPosition(It.IsAny<Mat>(), It.IsAny<Rect>()))
                    .Returns(() => (HeadRotation.Front, HeadRotation.Left));
                mHcFaceDetection.ReturnFace(_testUtils, "frontrotleft");
                yield return _nonEmptyFrame;
                mProgress.Verify(f => f.Report(It.Is(FaceNotStraight())), Times.Once());


                mHeadPositionService.Setup(f => f.GetHeadPosition(It.IsAny<Mat>(), It.IsAny<Rect>()))
                    .Returns(() => (HeadRotation.Front, HeadRotation.Right));
                yield return _nonEmptyFrame;
                mProgress.Verify(f => f.Report(It.Is(FaceNotStraight())), Times.Exactly(2));

                mHeadPositionService.Setup(f => f.GetHeadPosition(It.IsAny<Mat>(), It.IsAny<Rect>()))
                    .Returns(() => (HeadRotation.Front, HeadRotation.Front));
                mHcFaceDetection.ReturnFace(_testUtils, "front");
                yield return _nonEmptyFrame;


                mHcFaceDetection.Setup(f => f.DetectFrontalFaces(It.IsAny<Mat>())).Returns((new Rect[0], new Mat[0]));
                mHcFaceDetection.Setup(f => f.DetectFrontalThenProfileFaces(It.IsAny<Mat>()))
                    .Returns((new Rect[2], new Mat[2]));
                yield return _nonEmptyFrame;
                mProgress.Verify(f => f.Report(It.Is(ProfileFaceNotDetected())), Times.Once());


                mHcFaceDetection.ReturnFrontProfileFace(_testUtils, "left");
                mHeadPositionService.Setup(f => f.GetHeadPosition(It.IsAny<Mat>(), It.IsAny<Rect>()))
                    .Returns(() => (HeadRotation.Front, HeadRotation.Right));
                yield return _nonEmptyFrame;
                mProgress.Verify(f => f.Report(It.Is(FaceNotStraight())), Times.Exactly(3));


                mHeadPositionService.Setup(f => f.GetHeadPosition(It.IsAny<Mat>(), It.IsAny<Rect>()))
                    .Returns(() => (HeadRotation.Right, HeadRotation.Right));
                yield return _nonEmptyFrame;
                mProgress.Verify(f => f.Report(It.Is(FaceNotStraight())), Times.Exactly(4));

                mHeadPositionService.Setup(f => f.GetHeadPosition(It.IsAny<Mat>(), It.IsAny<Rect>()))
                    .Returns(() => (HeadRotation.Left, HeadRotation.Left));
                yield return _nonEmptyFrame;
                mProgress.Verify(f => f.Report(It.Is(FaceNotStraight())), Times.Exactly(4));


                mHcFaceDetection.ReturnFrontProfileFace(_testUtils, "right");
                mHeadPositionService.Setup(f => f.GetHeadPosition(It.IsAny<Mat>(), It.IsAny<Rect>()))
                    .Returns(() => (HeadRotation.Right, HeadRotation.Left));
                yield return _nonEmptyFrame;
                mProgress.Verify(f => f.Report(It.Is(FaceNotStraight())), Times.Exactly(5));


                mHeadPositionService.Setup(f => f.GetHeadPosition(It.IsAny<Mat>(), It.IsAny<Rect>()))
                    .Returns(() => (HeadRotation.Left, HeadRotation.Right));
                yield return _nonEmptyFrame;
                mProgress.Verify(f => f.Report(It.Is(FaceNotStraight())), Times.Exactly(6));

                mHeadPositionService.Setup(f => f.GetHeadPosition(It.IsAny<Mat>(), It.IsAny<Rect>()))
                    .Returns(() => (HeadRotation.Right, HeadRotation.Front));
                yield return _nonEmptyFrame;
                mProgress.Verify(f => f.Report(It.Is(FaceNotStraight())), Times.Exactly(6));
            }


            mCaptureService.Setup(f => f.CaptureFrames(It.IsAny<CancellationToken>())).Returns(InvalidCapture);
            var initTestService = _mocker.CreateInstance<InitFaceService>();
            initTestService.InitFaceProgress = mProgress.Object;
            await await initTestService.InitFace(mCaptureService.Object.CaptureFrames(CancellationToken.None).GetAsyncEnumerator(),
                CancellationToken.None);

            mTestImageRepository.Verify(f => f.Clear(), Times.Exactly(2));
            mLbphFaceRecognition.Verify(f => f.Reset(), Times.Exactly(2));
        }

        private IAsyncEnumerable<Mat> SuccessfulFaceCapturingScenario(Mock<IHcFaceDetection> mHcFaceDetection, Mock<IProgress<InitFaceProgressArgs>> mProgress, Mock<IHeadPositionService> mHeadPositionService)
        {
            async IAsyncEnumerable<Mat> SuccessfulCapture()
            {
                mHeadPositionService.Setup(f => f.GetHeadPosition(It.IsAny<Mat>(), It.IsAny<Rect>()))
                    .Returns(() => (HeadRotation.Front, HeadRotation.Front));
                mHcFaceDetection.ReturnFace(_testUtils, "front");
                yield return _nonEmptyFrame;
                mProgress.Verify(f => f.Report(It.Is(InitFaceProgress())), Times.Once());


                mHeadPositionService.Setup(f => f.GetHeadPosition(It.IsAny<Mat>(), It.IsAny<Rect>()))
                    .Returns(() => (HeadRotation.Left, HeadRotation.Left));
                mHcFaceDetection.ReturnFrontProfileFace(_testUtils, "left");
                yield return _nonEmptyFrame;
                mProgress.Verify(f => f.Report(It.Is(InitFaceProgress())), Times.AtLeast(2));


                mHeadPositionService.Setup(f => f.GetHeadPosition(It.IsAny<Mat>(), It.IsAny<Rect>()))
                    .Returns(() => (HeadRotation.Right, HeadRotation.Right));
                mHcFaceDetection.ReturnFrontProfileFace(_testUtils, "right");
                yield return _nonEmptyFrame;

                mProgress.Verify(f => f.Report(It.Is(InitFaceProgress())), Times.AtLeast(3));
            }

            return SuccessfulCapture();
        }

        [Fact]
        //success scenario
        public async Task InitFace_when_faces_captured_returns_valid_reports()
        {
            var mProgress = new Mock<IProgress<InitFaceProgressArgs>>();
            var mHcFaceDetection = SetupHcFaceDetection();
            var mTestImageRepository = SetupTestImageRepository();
            var mLbphFaceRecognition = SetupLbphFaceRecognition();
            var mCaptureService = SetupCaptureService();
            var mDnFaceRecognition = SetupDnFaceRecognition();
            var mHeadPositionService = SetupHeadPositionService();

            mDnFaceRecognition
                .Setup(f => f.CompareFaces(It.IsAny<Mat>(), It.IsAny<Mat>(), It.IsAny<Rect?>(), It.IsAny<Rect?>()))
                .Returns(true);

            mCaptureService.Setup(f => f.CaptureFrames(It.IsAny<CancellationToken>())).Returns(SuccessfulFaceCapturingScenario(mHcFaceDetection, mProgress, mHeadPositionService));
            mTestImageRepository.SetupGet(f => f.Count).Returns(0);

            var initTestService = _mocker.CreateInstance<InitFaceService>();
            initTestService.InitFaceProgress = mProgress.Object;
            await await initTestService.InitFace(mCaptureService.Object.CaptureFrames(CancellationToken.None).GetAsyncEnumerator(),
                CancellationToken.None);

            mProgress.Verify(f => f.Report(It.Is(InitFaceProgress(null, 100))), Times.Once());
            mTestImageRepository.Verify(f => f.Clear(), Times.Once());
            mLbphFaceRecognition.Verify(f => f.Reset(), Times.Once());
        }
    }
}