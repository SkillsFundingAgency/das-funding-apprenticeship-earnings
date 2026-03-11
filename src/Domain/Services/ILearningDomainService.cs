using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Models;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Services;

public interface ILearningDomainService
{
    Task<BaseLearning?> GetLearning(Guid key);
    Task Update(BaseLearning learning);
}
