using SFA.DAS.Funding.ApprenticeshipEarnings.DurableEntities.Models;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Factories
{
    public class ApprenticeshipFactory : IApprenticeshipFactory
    {
        public Apprenticeship.Apprenticeship CreateNew(ApprenticeshipEntityModel entityModel)
        {
            return new Apprenticeship.Apprenticeship(entityModel);
        }
    }
}
