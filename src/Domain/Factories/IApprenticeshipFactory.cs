using SFA.DAS.Learning.Types;
using SFA.DAS.Funding.ApprenticeshipEarnings.DataAccess.Entities;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Factories
{
    public interface IApprenticeshipFactory
    {
        Apprenticeship.Apprenticeship CreateNew(LearningCreatedEvent LearningCreatedEvent);
        Apprenticeship.Apprenticeship GetExisting(ApprenticeshipModel model);
    }
}
