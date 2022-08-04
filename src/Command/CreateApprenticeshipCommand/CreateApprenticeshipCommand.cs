using SFA.DAS.Funding.ApprenticeshipEarnings.DurableEntities.Models;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.Command.CreateApprenticeshipCommand
{
    public class CreateApprenticeshipCommand
    {
        public CreateApprenticeshipCommand(ApprenticeshipEntityModel apprenticeshipEntity)
        {
            ApprenticeshipEntity = apprenticeshipEntity;
        }

        public ApprenticeshipEntityModel ApprenticeshipEntity { get; }
    }
}
