using SFA.DAS.Funding.ApprenticeshipEarnings.Types;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.Command.CreateUnapprovedShortCourseLearningCommand;

public class CreateUnapprovedShortCourseLearningCommand : ICommand
{
    public CreateUnapprovedShortCourseLearningRequest Request { get; set; }
    public CreateUnapprovedShortCourseLearningCommand(CreateUnapprovedShortCourseLearningRequest request)
    {
        Request = request;
    }
}