namespace SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Repositories;

public interface IApprenticeshipRepository
{
    Task Add(Models.Learning apprenticeship);
    Task<Models.Learning?> Get(Guid key);
    Task Update(Models.Learning apprenticeship);
}