using System.Threading.Tasks;
using AutoFixture;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using SFA.DAS.Learning.Types;
using SFA.DAS.Funding.ApprenticeshipEarnings.DataTransferObjects;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Repositories;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Services;
using SFA.DAS.Funding.ApprenticeshipEarnings.Queries.GetAcademicYearEarnings;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.Queries.UnitTests.GetAcademicYearEarnings
{
    public class WhenGetAcademicYearEarnings
    {
        private Fixture _fixture;
        private Mock<IEarningsQueryRepository> _earningsQueryRepository;
        private Mock<IAcademicYearService> _academicYearService;
        private GetAcademicYearEarningsQueryHandler _sut;

        [SetUp]
        public void Setup()
        {
            _fixture = new Fixture();
            _earningsQueryRepository = new Mock<IEarningsQueryRepository>();
            _academicYearService = new Mock<IAcademicYearService>();
            _sut = new GetAcademicYearEarningsQueryHandler(_earningsQueryRepository.Object,
                _academicYearService.Object);
        }

        [Test]
        public async Task ThAcademicYearEarningsAreReturned()
        {
            var query = _fixture.Create<GetAcademicYearEarningsRequest>();
            var expectedResult = _fixture.Create<AcademicYearEarnings>();
            short currentAcademicYear = 2223;

            _academicYearService.Setup(x => x.CurrentAcademicYear).Returns(currentAcademicYear);
            _earningsQueryRepository.Setup(x => x.GetAcademicYearEarnings(query.Ukprn, currentAcademicYear))
                .ReturnsAsync(expectedResult);

            var actualResult = await _sut.Handle(query);

            actualResult.AcademicYearEarnings.Should().Be(expectedResult);
        }
    }
}