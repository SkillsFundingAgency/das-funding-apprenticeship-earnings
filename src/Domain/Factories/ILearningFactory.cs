using SFA.DAS.Learning.Types;
using SFA.DAS.Funding.ApprenticeshipEarnings.DataAccess.Entities;
using SFA.DAS.Funding.ApprenticeshipEarnings.Types;
using LearningDomainModel = SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Models.Learning;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Factories;

public interface ILearningFactory
{
    LearningDomainModel CreateNew(LearningCreatedEvent learningCreatedEvent, int fundingBandMaximum);
    LearningDomainModel GetExisting(LearningEntity model);
    LearningDomainModel CreateNewShortCourse(CreateUnapprovedShortCourseLearningRequest commandRequest);
}
