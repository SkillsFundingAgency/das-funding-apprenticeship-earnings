using SFA.DAS.Learning.Types;
using SFA.DAS.Funding.ApprenticeshipEarnings.DataAccess.Entities;
using SFA.DAS.Funding.ApprenticeshipEarnings.Types;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Factories
{
    public interface IApprenticeshipFactory
    {
        Apprenticeship.Apprenticeship CreateNew(LearningCreatedEvent learningCreatedEvent, int fundingBandMaximum);
        Apprenticeship.Apprenticeship GetExisting(LearningModel model);
        Apprenticeship.Apprenticeship CreateNewShortCourse(CreateUnapprovedShortCourseLearningRequest commandRequest);
    }
}
