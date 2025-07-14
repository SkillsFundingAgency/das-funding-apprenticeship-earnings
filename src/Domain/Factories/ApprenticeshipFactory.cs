using SFA.DAS.Learning.Types;
using SFA.DAS.Funding.ApprenticeshipEarnings.DataAccess.Entities;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Factories
{
    public class ApprenticeshipFactory : IApprenticeshipFactory
    {
        public Apprenticeship.Apprenticeship CreateNew(LearningCreatedEvent learningCreatedEvent)
        {
            return new Apprenticeship.Apprenticeship(learningCreatedEvent);
        }

        public Apprenticeship.Apprenticeship GetExisting(ApprenticeshipModel model)
        {
            return Apprenticeship.Apprenticeship.Get(model);
        }
    }
}
