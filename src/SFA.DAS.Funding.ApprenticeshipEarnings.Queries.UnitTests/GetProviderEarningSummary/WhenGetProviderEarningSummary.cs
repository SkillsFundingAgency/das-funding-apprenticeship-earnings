using System.Threading.Tasks;
using AutoFixture;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using SFA.DAS.Funding.ApprenticeshipEarnings.DataTransferObjects;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Repositories;
using SFA.DAS.Funding.ApprenticeshipEarnings.Queries.GetProviderEarningSummary;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.Queries.UnitTests.GetProviderEarningSummary
{
    public class WhenGetProviderEarningSummary
    {
        private Fixture _fixture;
        private Mock<IEarningsQueryRepository> _earningsQueryRepository;
        private GetProviderEarningSummaryQueryHandler _sut;

        [SetUp]
        public void Setup()
        {
            _fixture = new Fixture();
            _earningsQueryRepository = new Mock<IEarningsQueryRepository>();
            _sut = new GetProviderEarningSummaryQueryHandler(_earningsQueryRepository.Object);
        }

        [Test]
        public async Task TheSummaryIsReturned()
        {
            var query = _fixture.Create<GetProviderEarningSummaryRequest>();
            var expectedResult = _fixture.Create<ProviderEarningsSummary>();

            _earningsQueryRepository.Setup(x => x.GetProviderSummary(It.Is<long>(y => y == query.Ukprn))).ReturnsAsync(expectedResult);

            var actualResult = await _sut.Handle(query);

            actualResult.ProviderEarningsSummary.Should().Be(expectedResult);
        }
    }
}