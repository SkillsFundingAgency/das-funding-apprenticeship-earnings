using System;
using System.Threading.Tasks;
using AutoFixture;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using SFA.DAS.Funding.ApprenticeshipEarnings.DataAccess.ReadModel;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.DataAccess.UnitTests.EarningsQueryRepository
{
    public class WhenGetProviderSummary
    {
        private ApprenticeshipEarningsDataContext _dbContext;
        private Repositories.EarningsQueryRepository _sut;
        private Fixture _fixture;

        [SetUp]
        public void Setup()
        {
            _fixture = new Fixture();

            var options = new DbContextOptionsBuilder<ApprenticeshipEarningsDataContext>().UseInMemoryDatabase("EmployerIncentivesDbContext" + Guid.NewGuid()).Options;
            _dbContext = new ApprenticeshipEarningsDataContext(options);
            _sut = new Repositories.EarningsQueryRepository(new Lazy<ApprenticeshipEarningsDataContext>(_dbContext));
        }

        [TearDown]
        public void CleanUp() => _dbContext.Dispose();

        [Test]
        public async Task ThenCurrentAcademicYearTotalEarningsIsReturned()
        {
            var providerId = _fixture.Create<long>();
            short currentAcademicYear = 2223;

            var earnings = new Earning[]
            {
                _fixture.Build<Earning>().With(x => x.UKPRN, providerId).With(x => x.AcademicYear, currentAcademicYear).With(x => x.Amount, 1000).Create(), //Include
                _fixture.Build<Earning>().With(x => x.UKPRN, providerId).With(x => x.AcademicYear, currentAcademicYear).With(x => x.Amount, 2000).Create(), //Include
                _fixture.Build<Earning>().With(x => x.UKPRN, providerId).With(x => x.AcademicYear, currentAcademicYear + 1).With(x => x.Amount, 1000).Create(), //Exclude - wrong academic year
                _fixture.Build<Earning>().With(x => x.UKPRN, providerId).With(x => x.AcademicYear, currentAcademicYear).With(x => x.Amount, 2000).Create(), //Include
                _fixture.Build<Earning>().With(x => x.UKPRN, providerId + 1).With(x => x.AcademicYear, currentAcademicYear).With(x => x.Amount, 1000).Create() //Exclude - wrong provider
            };

            await _dbContext.AddRangeAsync(earnings);
            await _dbContext.SaveChangesAsync();

            // Act
            var result = await _sut.GetProviderSummary(providerId, currentAcademicYear);

            // Assert
            result.TotalEarningsForCurrentAcademicYear.Should().Be(5000);
        }
    }
}
