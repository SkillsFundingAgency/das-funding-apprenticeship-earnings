using SFA.DAS.Apprenticeships.Types;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.Command.CreateApprenticeshipCommand
{
    public class CreateApprenticeshipCommand : ICommand
    {
        public CreateApprenticeshipCommand(ApprenticeshipCreatedEvent apprenticeshipCreatedEvent)
        {
            ApprenticeshipCreatedEvent = apprenticeshipCreatedEvent;
        }

        public ApprenticeshipCreatedEvent ApprenticeshipCreatedEvent { get; }
    }
}
