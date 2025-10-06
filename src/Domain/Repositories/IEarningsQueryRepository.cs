using SFA.DAS.Funding.ApprenticeshipEarnings.DataTransferObjects;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Repositories;

public interface IEarningsQueryRepository
{
    Task Add(Apprenticeship.Apprenticeship apprenticeship);
    Task Replace(Apprenticeship.Apprenticeship apprenticeship);
    Task<ProviderEarningsSummary> GetProviderSummary(long ukprn, short academicYear);
    Task<AcademicYearEarnings> GetAcademicYearEarnings(long ukprn, short academicYear);
    List<Apprenticeship.Apprenticeship>? GetApprenticeships(long ukprn, DateTime searchDate, bool onlyActiveApprenticeships = false);
    Apprenticeship.Apprenticeship? GetApprenticeship(Guid learningKey, long ukprn, DateTime searchDate, bool onlyActiveApprenticeships = false);
}