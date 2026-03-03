namespace SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Repositories;

public interface IEarningsQueryRepository
{
    List<Models.Learning>? GetApprenticeships(long ukprn, DateTime searchDate, bool onlyActiveApprenticeships = false);
    List<Models.Learning> GetApprenticeships(List<Guid>? learningKeys, long ukprn, DateTime searchDate, bool onlyActiveApprenticeships = false);
}