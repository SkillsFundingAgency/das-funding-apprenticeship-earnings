namespace SFA.DAS.Funding.ApprenticeshipEarnings.Command.UpdateShortCourseOnProgrammeCommand;

public class UpdateShortCourseOnProgrammeCommand : ICommand
{
    public Guid LearningKey { get; set; }
    public UpdateShortCourseOnProgrammeRequest Request { get; set; }
    public UpdateShortCourseOnProgrammeCommand(Guid learningKey, UpdateShortCourseOnProgrammeRequest request)
    {
        LearningKey = learningKey;
        Request = request;
    }
}
