using SFA.DAS.Apprenticeships.Types;
using SFA.DAS.Funding.ApprenticeshipEarnings.DataAccess.Entities;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Factories
{
    public class ApprenticeshipFactory : IApprenticeshipFactory
    {
        public Apprenticeship.Apprenticeship CreateNew(ApprenticeshipCreatedEvent apprenticeshipCreatedEvent)
        {
            return new Apprenticeship.Apprenticeship(apprenticeshipCreatedEvent);
        }

        public Apprenticeship.Apprenticeship GetExisting(ApprenticeshipModel model)
        {
            return Apprenticeship.Apprenticeship.Get(model);
        }
    }
}
