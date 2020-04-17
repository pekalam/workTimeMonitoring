using System;
using Infrastructure.Repositories;
using OpenCvSharp;
using WorkTimeAlghorithm;
using Xunit;

namespace Infrastructure.Tests
{
    [Trait("Category", "Integration")]
    public class DefaultTestImageRepositoryTests : TestImageRepositoryTests
    {
        public override ITestImageRepository GetTestImageRepository()
        {
            return new DefaultTestImageRepository();
        }

        protected override TestImage CreateTestImage(bool isReferenceImg = true)
        {
            return new TestImage(new Rect(0, 0, 20, 20), Mat.Zeros(4, 4, MatType.CV_8UC1), HeadRotation.Left, DateTime.UtcNow, isReferenceImg);
        }
    }
}