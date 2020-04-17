using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using Moq.AutoMock;
using OpenCvSharp;
using WorkTimeAlghorithm.UnitTests.Helpers;
using Xunit;

namespace WorkTimeAlghorithm.UnitTests
{
    public class InitFaceServiceTests
    {
        private ImageTestUtils _testUtils = new ImageTestUtils();
        private AutoMocker _mocker = new AutoMocker();
        private readonly Mat _nonEmptyFrame = Mat.Zeros(600, 800, MatType.CV_32FC3);

        public InitFaceServiceTests()
        {
            TestImageBuilderFactory.Create = () => new TTestImageBuilder();
        }

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
                             args.ProgressState == WorkTimeAlghorithm.InitFaceProgress.FaceNotDetected &&
                             !args.Stoped;
        }

        private Expression<Func<InitFaceProgressArgs, bool>> ProfileFaceNotDetected()
        {
            return (args) => args.Frame == _nonEmptyFrame &&
                             args.ProgressState == WorkTimeAlghorithm.InitFaceProgress.ProfileFaceNotDetected &&
                             !args.Stoped;
        }

        private Expression<Func<InitFaceProgressArgs, bool>> PhotosTaken()
        {
            return (args) => args.Frame == null &&
                             args.ProgressPercentage > 0 &&
                             args.ProgressState == WorkTimeAlghorithm.InitFaceProgress.PhotosTaken &&
                             !args.Stoped;
        }

        private Expression<Func<InitFaceProgressArgs, bool>> InitFaceProgress()
        {
            return (args) => args.Frame == _nonEmptyFrame &&
                             args.ProgressPercentage > 0 &&
                             args.ProgressState == WorkTimeAlghorithm.InitFaceProgress.Progress &&
                             !args.Stoped;
        }

        private Expression<Func<InitFaceProgressArgs, bool>> InitFaceProgress(Mat frame, int progress)
        {
            return (args) => args.Frame == frame &&
                             args.ProgressPercentage == progress &&
                             args.ProgressState == WorkTimeAlghorithm.InitFaceProgress.Progress &&
                             !args.Stoped;
        }

        private Expression<Func<InitFaceProgressArgs, bool>> FaceNotStraight()
        {
            return (args) => args.Frame == _nonEmptyFrame &&
                             args.ProgressState == WorkTimeAlghorithm.InitFaceProgress.FaceNotStraight &&
                             !args.Stoped;
        }

        private Expression<Func<InitFaceProgressArgs, bool>> FaceNotLeft()
        {
            return (args) => args.Frame == _nonEmptyFrame &&
                             args.ProgressState == WorkTimeAlghorithm.InitFaceProgress.FaceNotTurnedLeft &&
                             !args.Stoped;
        }

        private Expression<Func<InitFaceProgressArgs, bool>> FaceNotRight()
        {
            return (args) => args.Frame == _nonEmptyFrame &&
                             args.ProgressState == WorkTimeAlghorithm.InitFaceProgress.FaceNotTurnedRight &&
                             !args.Stoped;
        }

        private Expression<Func<InitFaceProgressArgs, bool>> CancelledByUser()
        {
            return (args) =>
                args.Frame == null && args.ProgressState == WorkTimeAlghorithm.InitFaceProgress.CancelledByUser
                                   && args.Stoped;
        }

        private Expression<Func<InitFaceProgressArgs, bool>> FaceRecognitionError()
        {
            return (args) =>
                args.Frame == null && args.ProgressState == WorkTimeAlghorithm.InitFaceProgress.FaceRecognitionError
                                   && args.Stoped;
        }


        [Fact]
        public async Task InitFace_when_faces_not_captured_and_interrupted_returns_cancelled_progress()
        {
            var mProgress = new Mock<IProgress<InitFaceProgressArgs>>();
            var mHcFaceDetection = SetupHcFaceDetection();
            var mTestImageRepository = SetupTestImageRepository();
            var mCaptureService = SetupCaptureService();
            var mDnFaceRecognition = SetupDnFaceRecognition();
            SetupHeadPositionService();

            async IAsyncEnumerable<Mat> InterruptedCapture()
            {
                mHcFaceDetection.Setup(f => f.DetectFrontalFaces(It.IsAny<Mat>())).Returns(new Rect[0]);
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
        }

        [Fact]
        public async Task InitFace_when_dn_compare_faces_throws_returns_error_progress()
        {
            var mProgress = new Mock<IProgress<InitFaceProgressArgs>>();
            var mHcFaceDetection = SetupHcFaceDetection();
            var mTestImageRepository = SetupTestImageRepository();
            var mCaptureService = SetupCaptureService();
            var mDnFaceRecognition = SetupDnFaceRecognition();
            var mHeadPositionService = SetupHeadPositionService();


            mDnFaceRecognition.Setup(f => f.GetFaceEncodings(It.IsAny<Mat>())).Returns(FaceEncodingData.ValidFaceEncodingData);
            mDnFaceRecognition
                .Setup(f => f.CompareFaces(It.IsAny<Mat>(), It.IsAny<FaceEncodingData?>(), It.IsAny<Mat>(), It.IsAny<FaceEncodingData?>()))
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
        }

        [Fact]
        public async Task InitFace_when_gen_encoding_throws_returns_error_progress()
        {
            var mProgress = new Mock<IProgress<InitFaceProgressArgs>>();
            var mHcFaceDetection = SetupHcFaceDetection();
            var mTestImageRepository = SetupTestImageRepository();
            var mCaptureService = SetupCaptureService();
            var mDnFaceRecognition = SetupDnFaceRecognition();
            var mHeadPositionService = SetupHeadPositionService();

            mCaptureService.Setup(f => f.CaptureFrames(It.IsAny<CancellationToken>())).Returns(SuccessfulFaceCapturingScenario(mHcFaceDetection, mProgress, mHeadPositionService));
            var initTestService = _mocker.CreateInstance<InitFaceService>();
            initTestService.InitFaceProgress = mProgress.Object;
            await await initTestService.InitFace(mCaptureService.Object.CaptureFrames(CancellationToken.None).GetAsyncEnumerator(),
                CancellationToken.None);

            mProgress.Verify(f => f.Report(It.Is(InitFaceProgress(_nonEmptyFrame, 100))), Times.Never());
            mProgress.Verify(f => f.Report(It.Is(InitFaceProgress(null, 100))), Times.Never());
            mProgress.Verify(f => f.Report(It.Is(FaceRecognitionError())), Times.Once());

            mTestImageRepository.Verify(f => f.Clear(), Times.Exactly(2));
        }


        [Fact]
        public async Task InitFace_when_invalid_frames_returns_valid_progress_states()
        {
            var mProgress = new Mock<IProgress<InitFaceProgressArgs>>();
            var mHcFaceDetection = SetupHcFaceDetection();
            var mTestImageRepository = SetupTestImageRepository();
            var mCaptureService = SetupCaptureService();
            var mDnFaceRecognition = SetupDnFaceRecognition();
            var mHeadPositionService = SetupHeadPositionService();

            async IAsyncEnumerable<Mat> InvalidCapture()
            {
                mHcFaceDetection.Setup(f => f.DetectFrontalFaces(It.IsAny<Mat>())).Returns(new Rect[0]);
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
                mProgress.Verify(f => f.Report(It.Is(InitFaceProgress())), Times.Exactly(1));


                mHcFaceDetection.Setup(f => f.DetectFrontalFaces(It.IsAny<Mat>())).Returns(new Rect[0]);
                mHcFaceDetection.Setup(f => f.DetectFrontalThenProfileFaces(It.IsAny<Mat>()))
                    .Returns(new Rect[2]);
                yield return _nonEmptyFrame;
                mProgress.Verify(f => f.Report(It.Is(ProfileFaceNotDetected())), Times.Once());


                mHcFaceDetection.ReturnFrontProfileFace(_testUtils, "left");
                mHeadPositionService.Setup(f => f.GetHeadPosition(It.IsAny<Mat>(), It.IsAny<Rect>()))
                    .Returns(() => (HeadRotation.Front, HeadRotation.Right));
                yield return _nonEmptyFrame;
                mProgress.Verify(f => f.Report(It.Is(FaceNotLeft())), Times.Exactly(1));


                mHeadPositionService.Setup(f => f.GetHeadPosition(It.IsAny<Mat>(), It.IsAny<Rect>()))
                    .Returns(() => (HeadRotation.Right, HeadRotation.Right));
                yield return _nonEmptyFrame;
                mProgress.Verify(f => f.Report(It.Is(FaceNotLeft())), Times.Exactly(2));

                mHeadPositionService.Setup(f => f.GetHeadPosition(It.IsAny<Mat>(), It.IsAny<Rect>()))
                    .Returns(() => (HeadRotation.Left, HeadRotation.Left));
                yield return _nonEmptyFrame;
                mProgress.Verify(f => f.Report(It.Is(InitFaceProgress())), Times.Exactly(2));


                mHcFaceDetection.ReturnFrontProfileFace(_testUtils, "right");
                mHeadPositionService.Setup(f => f.GetHeadPosition(It.IsAny<Mat>(), It.IsAny<Rect>()))
                    .Returns(() => (HeadRotation.Right, HeadRotation.Left));
                yield return _nonEmptyFrame;
                mProgress.Verify(f => f.Report(It.Is(FaceNotRight())), Times.Exactly(1));


                mHeadPositionService.Setup(f => f.GetHeadPosition(It.IsAny<Mat>(), It.IsAny<Rect>()))
                    .Returns(() => (HeadRotation.Left, HeadRotation.Right));
                yield return _nonEmptyFrame;
                mProgress.Verify(f => f.Report(It.Is(FaceNotRight())), Times.Exactly(2));

            }


            mDnFaceRecognition.Setup(f => f.GetFaceEncodings(It.IsAny<Mat>())).Returns(FaceEncodingData.ValidFaceEncodingData);
            mDnFaceRecognition
                .Setup(f => f.CompareFaces(It.IsAny<Mat>(), It.IsAny<FaceEncodingData?>(), It.IsAny<Mat>(), It.IsAny<FaceEncodingData?>()))
                .Returns(true);

            mCaptureService.Setup(f => f.CaptureFrames(It.IsAny<CancellationToken>())).Returns(InvalidCapture);
            var initTestService = _mocker.CreateInstance<InitFaceService>();
            initTestService.InitFaceProgress = mProgress.Object;
            await await initTestService.InitFace(mCaptureService.Object.CaptureFrames(CancellationToken.None).GetAsyncEnumerator(),
                CancellationToken.None);

            mTestImageRepository.Verify(f => f.Clear(), Times.Exactly(2));
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

            }

            return SuccessfulCapture();
        }

        //success scenario
        [Fact]
        public async Task InitFace_when_faces_captured_returns_valid_reports()
        {
            var mProgress = new Mock<IProgress<InitFaceProgressArgs>>();
            var mHcFaceDetection = SetupHcFaceDetection();
            var mTestImageRepository = SetupTestImageRepository();
            var mCaptureService = SetupCaptureService();
            var mDnFaceRecognition = SetupDnFaceRecognition();
            var mHeadPositionService = SetupHeadPositionService();

            mDnFaceRecognition
                .Setup(f => f.CompareFaces(It.IsAny<Mat>(), It.IsAny<FaceEncodingData?>(), It.IsAny<Mat>(), It.IsAny<FaceEncodingData?>()))
                .Returns(true);

            mDnFaceRecognition.Setup(f => f.GetFaceEncodings(It.IsAny<Mat>())).Returns(FaceEncodingData.ValidFaceEncodingData);

            mCaptureService.Setup(f => f.CaptureFrames(It.IsAny<CancellationToken>())).Returns(SuccessfulFaceCapturingScenario(mHcFaceDetection, mProgress, mHeadPositionService));
            mTestImageRepository.SetupGet(f => f.Count).Returns(0);

            var initTestService = _mocker.CreateInstance<InitFaceService>();
            initTestService.InitFaceProgress = mProgress.Object;
            await await initTestService.InitFace(mCaptureService.Object.CaptureFrames(CancellationToken.None).GetAsyncEnumerator(),
                CancellationToken.None);


            mProgress.Verify(f => f.Report(It.Is(InitFaceProgress())), Times.AtLeast(3));
            mProgress.Verify(f => f.Report(It.Is(PhotosTaken())), Times.Exactly(1));

            mProgress.Verify(f => f.Report(It.Is(InitFaceProgress(null, 100))), Times.Once());
            mTestImageRepository.Verify(f => f.Clear(), Times.Once());
        }
    }
}