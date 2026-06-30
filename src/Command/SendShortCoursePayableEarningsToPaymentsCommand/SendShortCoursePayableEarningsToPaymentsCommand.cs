using SFA.DAS.Funding.ApprenticeshipEarnings.Types;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.Command.SendShortCoursePayableEarningsToPaymentsCommand;

public class SendShortCoursePayableEarningsToPaymentsCommand : ICommand
{
    public SendShortCoursePayableEarningsToPaymentsCommand(ShortCoursePayableEarningsUpdatedEvent shortCoursePayableEarningsUpdatedEvent)
    {
        ShortCoursePayableEarningsUpdatedEvent = shortCoursePayableEarningsUpdatedEvent;
    }

    public ShortCoursePayableEarningsUpdatedEvent ShortCoursePayableEarningsUpdatedEvent { get; }
}
