using SFA.DAS.Learning.Types;
using SFA.DAS.Funding.ApprenticeshipEarnings.DataAccess.Entities;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Factories
{
    public class ApprenticeshipFactory : IApprenticeshipFactory
    {
        public Apprenticeship.Apprenticeship CreateNew(LearningCreatedEvent LearningCreatedEvent)
        {
            return new Apprenticeship.Apprenticeship(LearningCreatedEvent);
        }

        public Apprenticeship.Apprenticeship GetExisting(ApprenticeshipModel model)
        {
            return Apprenticeship.Apprenticeship.Get(model);
        }
    }
}
