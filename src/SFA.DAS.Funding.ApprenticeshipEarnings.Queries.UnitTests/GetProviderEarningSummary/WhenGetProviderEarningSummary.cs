using System.Threading.Tasks;
using AutoFixture;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using SFA.DAS.Funding.ApprenticeshipEarnings.DataTransferObjects;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Repositories;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Services;
using SFA.DAS.Funding.ApprenticeshipEarnings.Queries.GetProviderEarningSummary;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.Queries.UnitTests.GetProviderEarningSummary
{
    public class WhenGetProviderEarningSummary
    {
        private Fixture _fixture;
        private Mock<IEarningsQueryRepository> _earningsQueryRepository;
        private Mock<IAcademicYearService> _academicYearService;
        private GetProviderEarningSummaryQueryHandler _sut;

        [SetUp]
        public void Setup()
        {
            _fixture = new Fixture();
            _earningsQueryRepository = new Mock<IEarningsQueryRepository>();
            _academicYearService = new Mock<IAcademicYearService>();
            _sut = new GetProviderEarningSummaryQueryHandler(_earningsQueryRepository.Object, _academicYearService.Object);
        }

        [Test]
        public async Task TheSummaryIsReturned()
        {
            var query = _fixture.Create<GetProviderEarningSummaryRequest>();
            var expectedResult = _fixture.Create<ProviderEarningsSummary>();
            short currentAcademicYear = 2223;

            _academicYearService.Setup(x => x.CurrentAcademicYear).Returns(currentAcademicYear);
            _earningsQueryRepository.Setup(x => x.GetProviderSummary(query.Ukprn, currentAcademicYear)).ReturnsAsync(expectedResult);

            var actualResult = await _sut.Handle(query);

            actualResult.ProviderEarningsSummary.Should().Be(expectedResult);
        }
    }
}