namespace SFA.DAS.Funding.ApprenticeshipEarnings.Command.SaveMathsAndEnglishCommand;

public class SaveMathsAndEnglishCommand : ICommand
{
    public SaveMathsAndEnglishCommand(Guid learningKey, SaveMathsAndEnglishRequest saveMathsAndEnglishRequest)
    {
        LearningKey = learningKey;
        MathsAndEnglishDetails = saveMathsAndEnglishRequest;
    }

    public Guid LearningKey { get; }

    public List<MathsAndEnglishDetail> MathsAndEnglishDetails { get; set; }
}

public class MathsAndEnglishDetail
{
    public DateTime StartDate { get; set; }

    public DateTime EndDate { get; set; }

    public string Course { get; set; } = null!;

    public decimal Amount { get; set; }
}