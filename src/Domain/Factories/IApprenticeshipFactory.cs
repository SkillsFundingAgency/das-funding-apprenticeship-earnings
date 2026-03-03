using SFA.DAS.Learning.Types;
using SFA.DAS.Funding.ApprenticeshipEarnings.DataAccess.Entities;
using SFA.DAS.Funding.ApprenticeshipEarnings.Types;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Factories
{
    public interface IApprenticeshipFactory
    {
        Models.Learning CreateNew(LearningCreatedEvent learningCreatedEvent, int fundingBandMaximum);
        Models.Learning GetExisting(LearningEntity model);
        Models.Learning CreateNewShortCourse(CreateUnapprovedShortCourseLearningRequest commandRequest);
    }
}
