using SFA.DAS.Funding.ApprenticeshipEarnings.Types;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.Command.ArchiveEarningsProfileCommand;


public class ArchiveEarningsProfileCommand : ICommand
{
    public ArchiveEarningsProfileCommand(EarningsProfileUpdatedEvent earningsProfileUpdatedEvent)
    {
        EarningsProfileUpdatedEvent = earningsProfileUpdatedEvent;
    }

    public EarningsProfileUpdatedEvent EarningsProfileUpdatedEvent { get; }
}