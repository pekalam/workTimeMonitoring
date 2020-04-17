namespace WorkTimeAlghorithm.UnitTests
{
    public class TTestImageBuilder : TestImageBuilder
    {
        public override TestImage Build()
        {
            return new TestImage(FaceLocation, Img, Rotation, DateCreated, true);
        }
    }
}