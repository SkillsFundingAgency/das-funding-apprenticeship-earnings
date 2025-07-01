using SFA.DAS.Learning.Types;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.Command.CreateApprenticeshipCommand
{
    public class CreateApprenticeshipCommand : ICommand
    {
        public CreateApprenticeshipCommand(LearningCreatedEvent LearningCreatedEvent)
        {
            LearningCreatedEvent = LearningCreatedEvent;
        }

        public LearningCreatedEvent LearningCreatedEvent { get; }
    }
}
