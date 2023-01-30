using Microsoft.EntityFrameworkCore;
using SFA.DAS.Apprenticeships.Types;
using SFA.DAS.Funding.ApprenticeshipEarnings.DataAccess.Mappers;
using SFA.DAS.Funding.ApprenticeshipEarnings.DataTransferObjects;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Apprenticeship;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Repositories;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.DataAccess.Repositories
{
    public class EarningsQueryRepository : IEarningsQueryRepository
    {
        private readonly Lazy<ApprenticeshipEarningsDataContext> _lazyContext;
        private ApprenticeshipEarningsDataContext DbContext => _lazyContext.Value;

        private const decimal GovernmentContribution = 0.95m;
        private const decimal EmployerContribution = 0.05m;

        public EarningsQueryRepository(Lazy<ApprenticeshipEarningsDataContext> dbContext)
        {
            _lazyContext = dbContext;
        }

        public async Task Add(Apprenticeship apprenticeship)
        {
            var earningsReadModels = apprenticeship.ToEarningsReadModels();
            await DbContext.AddRangeAsync(earningsReadModels);
            await DbContext.SaveChangesAsync();
        }

        public async Task<ProviderEarningsSummary> GetProviderSummary(long ukprn, short academicYear)
        {
            var dbResponse = new
            {
                levyEarnings = await DbContext.Earning.Where(x => x.UKPRN == ukprn && x.AcademicYear == academicYear && x.FundingType == FundingType.Levy).SumAsync(x => x.Amount),
                nonLevyEarnings = await DbContext.Earning.Where(x => x.UKPRN == ukprn && x.AcademicYear == academicYear && x.FundingType == FundingType.NonLevy).SumAsync(x => x.Amount),
                transferEarnings = await DbContext.Earning.Where(x => x.UKPRN == ukprn && x.AcademicYear == academicYear && x.FundingType == FundingType.Transfer).SumAsync(x => x.Amount)
            };

            var summary = new ProviderEarningsSummary
            {
                TotalLevyEarningsForCurrentAcademicYear = dbResponse.levyEarnings + dbResponse.transferEarnings,
                TotalNonLevyEarningsForCurrentAcademicYear = dbResponse.nonLevyEarnings
            };

            summary.TotalEarningsForCurrentAcademicYear = summary.TotalLevyEarningsForCurrentAcademicYear + summary.TotalNonLevyEarningsForCurrentAcademicYear;
            summary.TotalNonLevyEarningsForCurrentAcademicYearGovernment = summary.TotalNonLevyEarningsForCurrentAcademicYear * GovernmentContribution;
            summary.TotalNonLevyEarningsForCurrentAcademicYearEmployer = summary.TotalNonLevyEarningsForCurrentAcademicYear * EmployerContribution;

            return summary;
        }

        public async Task<AcademicYearEarnings> GetAcademicYearEarnings(long ukprn, short academicYear)
        {
            var earnings = DbContext.Earning.Where(x => x.UKPRN == ukprn && x.AcademicYear == academicYear).GroupBy(x => x.Uln);
            var result = new AcademicYearEarnings
            {
                Learners = await earnings.Select(x => new Learner
                {
                    Uln = x.Key,
                    FundingType = x.First().FundingType,
                    OnProgrammeEarnings = x.Select(y => new OnProgrammeEarning
                    {
                        AcademicYear = y.AcademicYear,
                        DeliveryPeriod = y.DeliveryPeriod,
                        Amount = y.Amount
                    }).ToList(),
                    TotalOnProgrammeEarnings = x.Sum(y => y.Amount)
                }).ToListAsync()
            };

            return result;
        }
    }
}
