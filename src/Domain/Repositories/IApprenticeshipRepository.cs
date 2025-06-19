using SFA.DAS.Funding.ApprenticeshipEarnings.DataAccess.Entities;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Repositories;

public interface IApprenticeshipRepository
{
    Task Add(Apprenticeship.Apprenticeship apprenticeship);
    Task Add(EarningsProfileHistoryModel earningsProfile);
    Task<Apprenticeship.Apprenticeship> Get(Guid key);
    Task Update(Apprenticeship.Apprenticeship apprenticeship);
}