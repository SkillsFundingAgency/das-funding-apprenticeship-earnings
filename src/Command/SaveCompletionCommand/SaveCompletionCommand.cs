namespace SFA.DAS.Funding.ApprenticeshipEarnings.Command.SaveCompletionCommand;

public class SaveCompletionCommand : ICommand
{
    public SaveCompletionCommand(Guid apprenticeshipKey, SaveCompletionRequest saveCompletionRequest)
    {
        ApprenticeshipKey = apprenticeshipKey;
        CompletionDetails = saveCompletionRequest;
    }

    public Guid ApprenticeshipKey { get; }

    public SaveCompletionRequest CompletionDetails { get; set; }
}