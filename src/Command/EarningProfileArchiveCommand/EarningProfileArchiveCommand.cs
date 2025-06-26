using SFA.DAS.Funding.ApprenticeshipEarnings.Types;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.Command.EarningProfileArchiveCommand;


public class EarningProfileArchiveCommand : ICommand
{
    public EarningProfileArchiveCommand(ArchiveEarningsProfileEvent archiveEarningsProfileEvent)
    {
        ArchiveEarningsProfileEvent = archiveEarningsProfileEvent;
    }

    public ArchiveEarningsProfileEvent ArchiveEarningsProfileEvent { get; }
}