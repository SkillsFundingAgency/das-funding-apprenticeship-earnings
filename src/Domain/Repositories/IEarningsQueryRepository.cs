using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Models.Apprenticeship;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Repositories;

public interface IEarningsQueryRepository
{
    List<ApprenticeshipLearning>? GetApprenticeships(long ukprn, DateTime searchDate, bool onlyActiveApprenticeships = false);
    List<ApprenticeshipLearning> GetApprenticeships(List<Guid>? learningKeys, long ukprn, DateTime searchDate, bool onlyActiveApprenticeships = false);
}