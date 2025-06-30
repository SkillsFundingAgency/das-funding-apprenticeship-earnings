using SFA.DAS.Funding.ApprenticeshipEarnings.Types;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.Command.ArchiveEarningsProfileCommand;


public class ArchiveEarningsProfileCommand : ICommand
{
    public ArchiveEarningsProfileCommand(ArchiveEarningsProfileEvent archiveEarningsProfileEvent)
    {
        ArchiveEarningsProfileEvent = archiveEarningsProfileEvent;
    }

    public ArchiveEarningsProfileEvent ArchiveEarningsProfileEvent { get; }
}