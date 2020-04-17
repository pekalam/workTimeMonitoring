using Moq;
using OpenCvSharp;

namespace WorkTimeAlghorithm.UnitTests.Helpers
{
    static class HcFaceDetectionTestHelpers
    {
        public static void ReturnFace(this Mock<IHcFaceDetection> mHcFaceDetection, ImageTestUtils testUtils,
            string imgName)
        {
            mHcFaceDetection.Setup(f => f.DetectFrontalFaces(It.IsAny<Mat>())).Returns(() =>
            {
                var ret = testUtils.GetFaceImg(imgName);
                return new[] { ret.faceRect };
            });
        }

        public static void ReturnFrontProfileFace(this Mock<IHcFaceDetection> mHcFaceDetection,
            ImageTestUtils testUtils, string imgName)
        {
            mHcFaceDetection.Setup(f => f.DetectFrontalThenProfileFaces(It.IsAny<Mat>())).Returns(() =>
            {
                var ret = testUtils.GetFaceImg(imgName);
                return new[] { ret.faceRect };
            });
        }
    }
}