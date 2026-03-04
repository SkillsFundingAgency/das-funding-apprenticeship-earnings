namespace SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Repositories;

public interface ILearningRepository
{
    Task Add(Models.Learning learning);
    Task<Models.Learning?> Get(Guid key);
    Task Update(Models.Learning learning);
}