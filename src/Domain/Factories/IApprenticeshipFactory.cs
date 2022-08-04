using SFA.DAS.Funding.ApprenticeshipEarnings.DurableEntities.Models;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Factories
{
    public interface IApprenticeshipFactory
    {
        Apprenticeship.Apprenticeship CreateNew(ApprenticeshipEntityModel entityModel);
    }
}
