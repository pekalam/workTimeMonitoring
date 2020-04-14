using Infrastructure.Repositories;

namespace UnitTests
{
    public class DefaultTestImageRepositoryTests : TestImageRepositoryTests
    {
        public override ITestImageRepository GetTestImageRepository()
        {
            return new DefaultTestImageRepository();
        }
    }
}