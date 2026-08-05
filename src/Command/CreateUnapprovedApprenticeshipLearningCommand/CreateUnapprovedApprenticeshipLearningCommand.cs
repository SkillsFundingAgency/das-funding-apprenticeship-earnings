using SFA.DAS.Funding.ApprenticeshipEarnings.Types;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.Command.CreateUnapprovedApprenticeshipLearningCommand;

public class CreateUnapprovedApprenticeshipLearningCommand : ICommand
{
    public CreateUnapprovedApprenticeshipLearningRequest Request { get; }

    public CreateUnapprovedApprenticeshipLearningCommand(CreateUnapprovedApprenticeshipLearningRequest request)
    {
        Request = request;
    }
}
