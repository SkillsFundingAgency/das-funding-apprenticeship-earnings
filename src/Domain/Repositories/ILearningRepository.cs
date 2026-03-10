using SFA.DAS.Funding.ApprenticeshipEarnings.DataAccess.Entities;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Models;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Models.Apprenticeship;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Models.ShortCourse;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Repositories;

public interface ILearningRepository
{
    Task Add<TEntity, TEpisode>(BaseLearning<TEntity, TEpisode> learning) where TEntity : BaseLearningEntity;
    Task<ShortCourseLearning?> GetShortCourseLearning(Guid key);
    Task<ApprenticeshipLearning?> GetApprenticeshipLearning(Guid key);
    Task Update(ShortCourseLearning learning);
    Task Update(ApprenticeshipLearning learning);
}