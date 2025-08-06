using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Calculations;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.Command.SaveMathsAndEnglishCommand;

public static class SaveMathsAndEnglishCommandExtensions
{
    public static GenerateMathsAndEnglishPaymentsCommand ToGenerateMathsAndEnglishPaymentsCommand(this MathsAndEnglishDetail mathsAndEnglishDetails)
    {
        return new GenerateMathsAndEnglishPaymentsCommand(
            mathsAndEnglishDetails.StartDate,
            mathsAndEnglishDetails.EndDate,
            mathsAndEnglishDetails.Course,
            mathsAndEnglishDetails.Amount,
            mathsAndEnglishDetails.WithdrawalDate,
            mathsAndEnglishDetails.ActualEndDate,
            mathsAndEnglishDetails.PriorLearningAdjustmentPercentage);
    }
}