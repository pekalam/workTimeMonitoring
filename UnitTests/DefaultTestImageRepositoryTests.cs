using Infrastructure.Repositories;
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
    }
}