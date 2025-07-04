using SFA.DAS.Learning.Types;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.Command.CreateApprenticeshipCommand
{
    public class CreateApprenticeshipCommand(LearningCreatedEvent learningCreatedEvent) : ICommand
    {
        public LearningCreatedEvent LearningCreatedEvent { get; } = learningCreatedEvent;
    }
}
