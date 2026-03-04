using SFA.DAS.Funding.ApprenticeshipEarnings.Types;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.Command.ArchiveShortCourseEarningsProfileCommand;


public class ArchiveShortCourseEarningsProfileCommand : ICommand
{
    public ArchiveShortCourseEarningsProfileCommand(ShortCourseEarningsProfileUpdatedEvent earningsProfileUpdatedEvent)
    {
        EarningsProfileUpdatedEvent = earningsProfileUpdatedEvent;
    }

    public ShortCourseEarningsProfileUpdatedEvent EarningsProfileUpdatedEvent { get; }
}