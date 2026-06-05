using SFA.DAS.Funding.ApprenticeshipEarnings.Types;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.Command.ProcessShortCoursePayableEarningsUpdatedCommand;

public class ProcessShortCoursePayableEarningsUpdatedCommand : ICommand
{
    public ProcessShortCoursePayableEarningsUpdatedCommand(ShortCoursePayableEarningsUpdatedEvent shortCoursePayableEarningsUpdatedEvent)
    {
        ShortCoursePayableEarningsUpdatedEvent = shortCoursePayableEarningsUpdatedEvent;
    }

    public ShortCoursePayableEarningsUpdatedEvent ShortCoursePayableEarningsUpdatedEvent { get; }
}
