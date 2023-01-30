using System.Threading.Tasks;
using AutoFixture;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;
using SFA.DAS.Funding.ApprenticeshipEarnings.Infrastructure.Queries;
using SFA.DAS.Funding.ApprenticeshipEarnings.InnerApi.Controllers;
using SFA.DAS.Funding.ApprenticeshipEarnings.Queries.GetAcademicYearEarnings;

namespace SFA.DAS.ApprenticeshipEarnings.InnerApi.UnitTests.Controllers.ProviderEarningsControllerTests
{
    public class WhenGetDetail
    {
        private Fixture _fixture;
        private Mock<IQueryDispatcher> _queryDispatcher;
        private ProviderEarningsController _sut;

        [SetUp]
        public void Setup()
        {
            _fixture = new Fixture();
            _queryDispatcher = new Mock<IQueryDispatcher>();
            _sut = new ProviderEarningsController(_queryDispatcher.Object);
        }

        [Test]
        public async Task ThenTheAcademicYearEarningsAreReturned()
        {
            var ukprn = _fixture.Create<long>();
            var expectedResult = _fixture.Create<GetAcademicYearEarningsResponse>();

            _queryDispatcher.Setup(x => x.Send<GetAcademicYearEarningsRequest, GetAcademicYearEarningsResponse>(It.Is<GetAcademicYearEarningsRequest>(r => r.Ukprn == ukprn))).ReturnsAsync(expectedResult);

            var result = await _sut.Detail(ukprn);

            result.Should().BeOfType<OkObjectResult>();
            var okResult = (OkObjectResult)result;
            okResult.Value.Should().Be(expectedResult.AcademicYearEarnings);
        }
    }
}