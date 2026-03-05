namespace SFA.DAS.Funding.ApprenticeshipEarnings.Command.UpdateOnProgrammeCommand;

public class UpdateOnProgrammeCommand : ICommand
{
    public Guid LearningKey { get; internal set; }
    public UpdateOnProgrammeRequest Request { get; set; }

    public UpdateOnProgrammeCommand(Guid learningKey, UpdateOnProgrammeRequest request)
    {
        LearningKey = learningKey;
        Request = request;
    }
}
