using System.Threading.Tasks;
using AutoFixture;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;
using SFA.DAS.Funding.ApprenticeshipEarnings.Infrastructure.Queries;
using SFA.DAS.Funding.ApprenticeshipEarnings.InnerApi.Controllers;
using SFA.DAS.Funding.ApprenticeshipEarnings.Queries.GetProviderEarningSummary;

namespace SFA.DAS.ApprenticeshipEarnings.InnerApi.UnitTests.Controllers.ProviderEarningsControllerTests
{
    public class WhenGetSummary
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
        public async Task ThenTheProviderSummaryIsReturned()
        {
            var ukprn = _fixture.Create<long>();
            var expectedResult = _fixture.Create<GetProviderEarningSummaryResponse>();

            _queryDispatcher.Setup(x => x.Send<GetProviderEarningSummaryRequest, GetProviderEarningSummaryResponse>(It.Is<GetProviderEarningSummaryRequest>(r => r.Ukprn == ukprn))).ReturnsAsync(expectedResult);

            var result = await _sut.Summary(ukprn);

            result.Should().BeOfType<OkObjectResult>();
            var okResult = (OkObjectResult)result;
            okResult.Value.Should().Be(expectedResult.ProviderEarningsSummary);
        }
    }
}