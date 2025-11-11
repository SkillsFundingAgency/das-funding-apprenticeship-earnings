namespace SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Repositories;

public interface IEarningsQueryRepository
{
    List<Apprenticeship.Apprenticeship>? GetApprenticeships(long ukprn, DateTime searchDate, bool onlyActiveApprenticeships = false);
    List<Apprenticeship.Apprenticeship> GetApprenticeships(List<Guid>? learningKeys, long ukprn, DateTime searchDate, bool onlyActiveApprenticeships = false);
}