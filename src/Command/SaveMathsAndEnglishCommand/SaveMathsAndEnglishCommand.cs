namespace SFA.DAS.Funding.ApprenticeshipEarnings.Command.SaveMathsAndEnglishCommand;

public class SaveMathsAndEnglishCommand : ICommand
{
    public SaveMathsAndEnglishCommand(Guid apprenticeshipKey, SaveMathsAndEnglishRequest saveMathsAndEnglishRequest)
    {
        ApprenticeshipKey = apprenticeshipKey;
        MathsAndEnglishDetails = saveMathsAndEnglishRequest;
    }

    public Guid ApprenticeshipKey { get; }

    public List<MathsAndEnglishDetail> MathsAndEnglishDetails { get; set; }
}

public class MathsAndEnglishDetail
{
    public DateTime StartDate { get; set; }

    public DateTime EndDate { get; set; }

    public string Course { get; set; } = null!;

    public decimal Amount { get; set; }
    public DateTime? WithdrawalDate { get; set; }
    public int? PriorLearningAdjustmentPercentage { get; set; }
}