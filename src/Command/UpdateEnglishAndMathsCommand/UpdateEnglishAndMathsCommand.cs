namespace SFA.DAS.Funding.ApprenticeshipEarnings.Command.UpdateEnglishAndMathsCommand;

public class UpdateEnglishAndMathsCommand : ICommand
{
    public UpdateEnglishAndMathsCommand(Guid apprenticeshipKey, UpdateEnglishAndMathsRequest updateMathsAndEnglishRequest)
    {
        ApprenticeshipKey = apprenticeshipKey;
        EnglishAndMathsDetails = updateMathsAndEnglishRequest.EnglishAndMaths;
    }

    public Guid ApprenticeshipKey { get; }

    public List<EnglishAndMathsItem> EnglishAndMathsDetails { get; set; }
}
