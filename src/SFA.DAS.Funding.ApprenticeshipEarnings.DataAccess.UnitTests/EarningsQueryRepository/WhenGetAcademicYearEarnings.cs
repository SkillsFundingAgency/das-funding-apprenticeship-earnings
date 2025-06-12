using System;
using System.Linq;
using System.Threading.Tasks;
using AutoFixture;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;
using NUnit.Framework;
using SFA.DAS.Funding.ApprenticeshipEarnings.Configuration.Configuration;
using SFA.DAS.Funding.ApprenticeshipEarnings.DataAccess.ReadModel;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Services;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.DataAccess.UnitTests.EarningsQueryRepository
{
    public class WhenGetAcademicYearEarnings
    {
        private ApprenticeshipEarningsDataContext _dbContext;
        private Domain.Repositories.EarningsQueryRepository _sut;
        private Mock<IAcademicYearService> _mockAcademicYearService;
        private Fixture _fixture;

        [SetUp]
        public void Setup()
        {
            _fixture = new Fixture();
            _mockAcademicYearService = new Mock<IAcademicYearService>();
            var options = new DbContextOptionsBuilder<ApprenticeshipEarningsDataContext>().UseInMemoryDatabase("EmployerIncentivesDbContext" + Guid.NewGuid()).Options;
            _dbContext = new ApprenticeshipEarningsDataContext(new ApplicationSettings(), options);
            _sut = new Domain.Repositories.EarningsQueryRepository(new Lazy<ApprenticeshipEarningsDataContext>(_dbContext), Mock.Of<ISystemClockService>(), _mockAcademicYearService.Object);
        }

        [TearDown]
        public void CleanUp() => _dbContext.Dispose();

        [Test]
        public async Task ThenCurrentAcademicYearsEarningsAreReturnedForEachLearner()
        {
            var providerId = _fixture.Create<long>();
            short currentAcademicYear = 2223;
            var learner1Uln = "34356445";
            var learner2Uln = "43598546";

            var earnings = new Earning[]
            {
                _fixture.Build<Earning>().With(x => x.UKPRN, providerId).With(x => x.Uln, learner1Uln).With(x => x.AcademicYear, currentAcademicYear).With(x => x.Amount, 1001).With(x => x.IsNonLevyFullyFunded, false).Create(), //Include
                _fixture.Build<Earning>().With(x => x.UKPRN, providerId).With(x => x.Uln, learner1Uln).With(x => x.AcademicYear, currentAcademicYear).With(x => x.Amount, 2010).With(x => x.IsNonLevyFullyFunded, false).Create(), //Include
                _fixture.Build<Earning>().With(x => x.UKPRN, providerId).With(x => x.Uln, learner1Uln).With(x => x.AcademicYear, currentAcademicYear + 1).With(x => x.Amount, 1100).With(x => x.IsNonLevyFullyFunded, false).Create(), //Exclude - wrong academic year
                _fixture.Build<Earning>().With(x => x.UKPRN, providerId).With(x => x.Uln, learner2Uln).With(x => x.AcademicYear, currentAcademicYear).With(x => x.Amount, 2500).With(x => x.IsNonLevyFullyFunded, true).Create(), //Include
                _fixture.Build<Earning>().With(x => x.UKPRN, providerId + 1).With(x => x.Uln, learner2Uln).With(x => x.AcademicYear, currentAcademicYear).With(x => x.Amount, 1009).With(x => x.IsNonLevyFullyFunded, true).Create() //Exclude - wrong provider
            };

            await _dbContext.AddRangeAsync(earnings);
            await _dbContext.SaveChangesAsync();

            // Act
            var result = await _sut.GetAcademicYearEarnings(providerId, currentAcademicYear);

            // Assert
            result.Learners.Count.Should().Be(2);
            var learner1Result = result.Learners.Single(x => x.Uln == learner1Uln);
            learner1Result.Uln.Should().Be(learner1Uln);
            learner1Result.OnProgrammeEarnings.Count.Should().Be(2);
            learner1Result.OnProgrammeEarnings.Any(x => x.AcademicYear == currentAcademicYear && x.DeliveryPeriod == earnings[0].DeliveryPeriod && x.Amount == earnings[0].Amount).Should().BeTrue();
            learner1Result.OnProgrammeEarnings.Any(x => x.AcademicYear == currentAcademicYear && x.DeliveryPeriod == earnings[1].DeliveryPeriod && x.Amount == earnings[1].Amount).Should().BeTrue();
            learner1Result.TotalOnProgrammeEarnings.Should().Be(3011);
            learner1Result.IsNoneLevyFullyFunded.Should().BeFalse();

            var learner2Result = result.Learners.Single(x => x.Uln == learner2Uln);
            learner2Result.Uln.Should().Be(learner2Uln);
            learner2Result.OnProgrammeEarnings.Count.Should().Be(1);
            learner2Result.OnProgrammeEarnings.Any(x => x.AcademicYear == currentAcademicYear && x.DeliveryPeriod == earnings[3].DeliveryPeriod && x.Amount == earnings[3].Amount).Should().BeTrue();
            learner2Result.TotalOnProgrammeEarnings.Should().Be(2500);
            learner2Result.IsNoneLevyFullyFunded.Should().BeTrue();
        }
    }
}
