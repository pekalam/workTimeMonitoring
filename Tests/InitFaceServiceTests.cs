using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Infrastructure.src.WorkTime;
using Infrastructure.WorkTime;
using Moq;
using Moq.AutoMock;
using OpenCvSharp;
using Xunit;

namespace Tests
{
    static class TestUtilsHelpers
    {
        public static int AsInt(this string str)
        {
            return Convert.ToInt32(str);
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


        [Fact]
        public async Task InitFace_when_faces_not_captured_and_interrupted_throws()
        {
            var mProgress = new Mock<IProgress<InitFaceProgressArgs>>();
            var mHcFaceDetection = SetupHcFaceDetection();
            var mTestImageRepository = SetupTestImageRepository();
            var mLbphFaceRecognition = SetupLbphFaceRecognition();
            var mCaptureService = SetupCaptureService();
            var mDnFaceRecognition = SetupDnFaceRecognition();

            async IAsyncEnumerable<Mat> InterruptedCapture()
            {
                mHcFaceDetection.Setup(f => f.DetectFrontalFaces(It.IsAny<Mat>())).Returns((new Rect[0], new Mat[0]));
                yield return _nonEmptyFrame;
                yield return _nonEmptyFrame;
            }

            mCaptureService.Setup(f => f.CaptureFrames(It.IsAny<CancellationToken>())).Returns(InterruptedCapture);
            mTestImageRepository.SetupGet(f => f.Count).Returns(0);

            var initTestService = _mocker.CreateInstance<InitFaceService>();

            initTestService.InitFaceProgress = mProgress.Object;

            await Assert.ThrowsAnyAsync<Exception>(() => initTestService.InitFace(new CancellationTokenSource()));

            mTestImageRepository.Verify(f => f.Clear(), Times.Exactly(2));
            mLbphFaceRecognition.Verify(f => f.Reset(), Times.Exactly(2));
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

            mDnFaceRecognition.Setup(f => f.CompareFaces(It.IsAny<Mat>(), It.IsAny<Mat>())).Returns(true);

            async IAsyncEnumerable<Mat> SuccessfulCapture()
            {
                mHcFaceDetection.Setup(f => f.DetectFrontalFaces(It.IsAny<Mat>())).Returns((new Rect[0], new Mat[0]));
                yield return _nonEmptyFrame;

                mProgress.Verify(f => f.Report(It.Is(FaceNotDetected())), Times.Once());


                mHcFaceDetection.Setup(f => f.DetectFrontalFaces(It.IsAny<Mat>())).Returns(() =>
                {
                    var ret = _testUtils.GetFaceImg("front");
                    return (new[] { ret.faceRect }, new[] { ret.faceImg });
                });
                yield return _nonEmptyFrame;
                
                mProgress.Verify(f => f.Report(It.Is(InitFaceProgress())), Times.Once());

                
                
                mHcFaceDetection.Setup(f => f.DetectFrontalFaces(It.IsAny<Mat>())).Returns((new Rect[0], new Mat[0]));
                mHcFaceDetection.Setup(f => f.DetectFrontalThenProfileFaces(It.IsAny<Mat>())).Returns((new Rect[2], new Mat[2]));
                yield return _nonEmptyFrame;

                mProgress.Verify(f => f.Report(It.Is(ProfileFaceNotDetected())), Times.Once());



                mHcFaceDetection.Setup(f => f.DetectFrontalThenProfileFaces(It.IsAny<Mat>())).Returns(() =>
                {
                    var ret = _testUtils.GetFaceImg("left");
                    return (new[] { ret.faceRect }, new[] { ret.faceImg });
                });
                yield return _nonEmptyFrame;

                mProgress.Verify(f => f.Report(It.Is(InitFaceProgress())), Times.AtLeast(2));


                mHcFaceDetection.Setup(f => f.DetectFrontalThenProfileFaces(It.IsAny<Mat>())).Returns((new Rect[0], new Mat[0]));
                yield return _nonEmptyFrame;

                mProgress.Verify(f => f.Report(It.Is(ProfileFaceNotDetected())), Times.Exactly(2));

                mHcFaceDetection.Setup(f => f.DetectFrontalThenProfileFaces(It.IsAny<Mat>())).Returns(() =>
                {
                    var ret = _testUtils.GetFaceImg("right");
                    return (new[] { ret.faceRect }, new[] { ret.faceImg });
                });
                yield return _nonEmptyFrame;

                mProgress.Verify(f => f.Report(It.Is(InitFaceProgress())), Times.AtLeast(3));
            }

            mCaptureService.Setup(f => f.CaptureFrames(It.IsAny<CancellationToken>())).Returns(SuccessfulCapture);
            mTestImageRepository.SetupGet(f => f.Count).Returns(0);

            var initTestService = _mocker.CreateInstance<InitFaceService>();
            initTestService.InitFaceProgress = mProgress.Object;
            await initTestService.InitFace(new CancellationTokenSource());

            mProgress.Verify(f => f.Report(It.Is(InitFaceProgress(null, 99))), Times.Once());
            mTestImageRepository.Verify(f => f.Clear(), Times.Once());
            mLbphFaceRecognition.Verify(f => f.Reset(), Times.Once());
        }
    }


    public class HeadPositionServiceTests
    {
        [Fact]
        public void f()
        {
            var servicee = new HeadPositionService();

            var img = new ImageTestUtils().GetFaceImg("front");

            servicee.GetHeadPosition(img.faceImg, img.faceRect);
        }
    }
}