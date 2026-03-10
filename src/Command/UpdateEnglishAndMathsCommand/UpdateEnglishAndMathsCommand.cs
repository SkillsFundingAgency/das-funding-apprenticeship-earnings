namespace SFA.DAS.Funding.ApprenticeshipEarnings.Command.UpdateEnglishAndMathsCommand;

public class UpdateEnglishAndMathsCommand : ICommand
{
    public UpdateEnglishAndMathsCommand(Guid learningKey, UpdateEnglishAndMathsRequest updateMathsAndEnglishRequest)
    {
        LearningKey = learningKey;
        EnglishAndMathsDetails = updateMathsAndEnglishRequest.EnglishAndMaths;
    }

    public Guid LearningKey { get; }

    public List<EnglishAndMathsItem> EnglishAndMathsDetails { get; set; }
}
