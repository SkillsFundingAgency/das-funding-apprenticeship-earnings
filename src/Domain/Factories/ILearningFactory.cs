using SFA.DAS.Funding.ApprenticeshipEarnings.DataAccess.Entities.Apprenticeship;
using SFA.DAS.Funding.ApprenticeshipEarnings.DataAccess.Entities.ShortCourse;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Models.Apprenticeship;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Models.ShortCourse;
using SFA.DAS.Funding.ApprenticeshipEarnings.Types;
using SFA.DAS.Learning.Types;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Factories;

public interface ILearningFactory
{
    ApprenticeshipLearning CreateNew(LearningCreatedEvent learningCreatedEvent, int fundingBandMaximum);
    ApprenticeshipLearning GetExistingApprenticeship(ApprenticeshipLearningEntity model);
    ShortCourseLearning GetExistingShortCourse(ShortCourseLearningEntity model);
    ShortCourseLearning CreateNewShortCourse(CreateUnapprovedShortCourseLearningRequest commandRequest);
}
