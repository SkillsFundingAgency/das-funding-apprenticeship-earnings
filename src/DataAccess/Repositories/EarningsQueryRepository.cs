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

        public async Task<ProviderEarningsSummary> GetProviderSummary(long ukprn, short currentAcademicYear)
        {
            var summary = new ProviderEarningsSummary
            {
                TotalLevyEarningsForCurrentAcademicYear = await DbContext.Earning.Where(x => x.UKPRN == ukprn && x.AcademicYear == currentAcademicYear && x.FundingType == FundingType.Levy).SumAsync(x => x.Amount),
                TotalNonLevyEarningsForCurrentAcademicYear = await DbContext.Earning.Where(x => x.UKPRN == ukprn && x.AcademicYear == currentAcademicYear && x.FundingType == FundingType.NonLevy).SumAsync(x => x.Amount),
            };

            summary.TotalEarningsForCurrentAcademicYear = summary.TotalLevyEarningsForCurrentAcademicYear + summary.TotalNonLevyEarningsForCurrentAcademicYear;

            return summary;
        }
    }
}
