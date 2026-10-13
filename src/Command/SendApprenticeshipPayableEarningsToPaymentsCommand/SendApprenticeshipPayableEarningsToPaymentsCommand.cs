using SFA.DAS.Funding.ApprenticeshipEarnings.Types;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.Command.SendApprenticeshipPayableEarningsToPaymentsCommand;

public class SendApprenticeshipPayableEarningsToPaymentsCommand : ICommand
{
    public SendApprenticeshipPayableEarningsToPaymentsCommand(ApprenticeshipPayableEarningsUpdatedEvent apprenticeshipPayableEarningsUpdatedEvent)
    {
        ApprenticeshipPayableEarningsUpdatedEvent = apprenticeshipPayableEarningsUpdatedEvent;
    }

    public ApprenticeshipPayableEarningsUpdatedEvent ApprenticeshipPayableEarningsUpdatedEvent { get; }
}
