namespace SFA.DAS.Funding.ApprenticeshipEarnings.Command.ProcessWithdrawMathsAndEnglishCommand;

public class ProcessWithdrawnMathsAndEnglishCommand(Guid apprenticeshipKey, MathsAndEnglishWithdrawRequest withdrawRequest) : ICommand
{
    public Guid ApprenticeshipKey { get; set; } = apprenticeshipKey;
    public DateTime WithdrawalDate { get; set; } = withdrawRequest.WithdrawalDate;
    public string Course { get; set; } = withdrawRequest.Course;
}