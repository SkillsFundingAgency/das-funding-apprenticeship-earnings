using SFA.DAS.Funding.ApprenticeshipEarnings.Command.ProcessWithdrawnApprenticeshipCommand;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.Command.ProcessWithdrawMathsAndEnglishCommand;

public class MathsAndEnglishWithdrawRequest
{
    public string Course { get; set; }
    public DateTime WithdrawalDate { get; set; }
}