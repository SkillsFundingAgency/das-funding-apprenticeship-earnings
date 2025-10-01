using SFA.DAS.Learning.Types;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.Command.ProcessWithdrawnApprenticeshipCommand;

public class ProcessWithdrawnApprenticeshipCommand(Guid apprenticeshipKey, WithdrawRequest withdrawRequest) : ICommand
{
    public Guid ApprenticeshipKey { get; set; } = apprenticeshipKey;
    public DateTime WithdrawalDate { get; set; } = withdrawRequest.WithdrawalDate;
}