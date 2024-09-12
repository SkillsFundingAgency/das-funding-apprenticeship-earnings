using SFA.DAS.Apprenticeships.Types;
using SFA.DAS.Funding.ApprenticeshipEarnings.DataAccess.Entities;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Factories
{
    public interface IApprenticeshipFactory
    {
        Apprenticeship.Apprenticeship CreateNew(ApprenticeshipCreatedEvent apprenticeshipCreatedEvent);
        Apprenticeship.Apprenticeship GetExisting(ApprenticeshipModel model);
    }
}
