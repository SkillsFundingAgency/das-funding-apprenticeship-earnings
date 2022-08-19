using SFA.DAS.Funding.ApprenticeshipEarnings.DurableEntities.Models;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Factories
{
    public class ApprenticeshipFactory : IApprenticeshipFactory
    {
        public Apprenticeship.Apprenticeship CreateNew(ApprenticeshipEntityModel entityModel)
        {
            return new Apprenticeship.Apprenticeship(
                entityModel.ApprenticeshipKey,
                entityModel.ApprovalsApprenticeshipId,
                entityModel.Uln,
                entityModel.UKPRN,
                entityModel.EmployerAccountId,
                entityModel.LegalEntityName,
                entityModel.ActualStartDate,
                entityModel.PlannedEndDate,
                entityModel.AgreedPrice,
                entityModel.TrainingCode,
                entityModel.FundingEmployerAccountId,
                entityModel.FundingType,
                entityModel.AgeAtStartOfApprenticeship
            );
        }
    }
}
