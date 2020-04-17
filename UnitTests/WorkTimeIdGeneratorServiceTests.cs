using Domain;
using Domain.Services;
using FluentAssertions;
using Xunit;

namespace Infrastructure.Tests
{
    public abstract class WorkTimeIdGeneratorServiceTests
    {
        protected IWorkTimeIdGeneratorService _generatorService;

        public WorkTimeIdGeneratorServiceTests()
        {
            _generatorService = Create();
        }

        protected abstract IWorkTimeIdGeneratorService Create();

        [Fact]
        public void GenerateId_generates_unique_greater_than0_ids()
        {
            var id1 = _generatorService.GenerateId();

            id1.Should().BeGreaterThan(0);


            var id2 = _generatorService.GenerateId();


            id2.Should().BeGreaterThan(0);
            id1.Should().NotBe(id2, "Should be unique");
        }
    }
}