using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Models;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Models.Apprenticeship;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Models.ShortCourse;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Repositories;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Services;

public class LearningDomainService(ILearningRepository repository) : ILearningDomainService
{
    public async Task<BaseLearning?> GetLearning(Guid key)
        => (BaseLearning?) await repository.GetApprenticeshipLearning(key)
           ?? await repository.GetShortCourseLearning(key);

    public async Task Update(BaseLearning learning)
    {
        switch (learning)
        {
            case ApprenticeshipLearning apprenticeshipLearning: await repository.Update(apprenticeshipLearning); break;
            case ShortCourseLearning shortCourseLearning:    await repository.Update(shortCourseLearning); break;
            default: throw new InvalidOperationException($"Unknown learning type: {learning.GetType()}");
        }
    }
}
