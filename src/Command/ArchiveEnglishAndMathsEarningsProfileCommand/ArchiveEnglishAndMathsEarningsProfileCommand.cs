using SFA.DAS.Funding.ApprenticeshipEarnings.Types;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.Command.ArchiveEnglishAndMathsEarningsProfileCommand;


public class ArchiveEnglishAndMathsEarningsProfileCommand : ICommand
{
    public ArchiveEnglishAndMathsEarningsProfileCommand(EarningsProfileForEnglishAndMathsUpdatedEvent earningsProfileForEnglishAndMathsUpdatedEvent)
    {
        EarningsProfileForEnglishAndMathsUpdatedEvent = earningsProfileForEnglishAndMathsUpdatedEvent;
    }

    public EarningsProfileForEnglishAndMathsUpdatedEvent EarningsProfileForEnglishAndMathsUpdatedEvent { get; }
}
