using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Calculations;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.Command.UpdateEnglishAndMathsCommand;

public static class UpdateEnglishAndMathsCommandExtensions
{
    public static GenerateMathsAndEnglishPaymentsCommand ToGenerateMathsAndEnglishPaymentsCommand(this EnglishAndMathsItem mathsAndEnglishDetails)
    {
        return new GenerateMathsAndEnglishPaymentsCommand(
            mathsAndEnglishDetails.StartDate,
            mathsAndEnglishDetails.EndDate,
            mathsAndEnglishDetails.Course,
            mathsAndEnglishDetails.Amount,
            mathsAndEnglishDetails.WithdrawalDate,
            mathsAndEnglishDetails.ActualEndDate,
            mathsAndEnglishDetails.PauseDate,
            mathsAndEnglishDetails.PriorLearningAdjustmentPercentage);
    }
}