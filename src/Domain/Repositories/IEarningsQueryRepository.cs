using SFA.DAS.Funding.ApprenticeshipEarnings.DataTransferObjects;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.ApprenticeshipFunding;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Repositories
{
    public interface IEarningsQueryRepository
    {
        Task Add(Apprenticeship.Apprenticeship apprenticeship);
        Task Replace(Apprenticeship.Apprenticeship apprenticeship);
        Task<ProviderEarningsSummary> GetProviderSummary(long ukprn, short academicYear);
        Task<AcademicYearEarnings> GetAcademicYearEarnings(long ukprn, short academicYear);
    }
}
