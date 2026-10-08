using SFA.DAS.Funding.ApprenticeshipEarnings.Types;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.Command.ArchiveEnglishAndMathsEarningsProfileCommand;


public class ArchiveEnglishAndMathsEarningsProfileCommand : ICommand
{
    public ArchiveEnglishAndMathsEarningsProfileCommand(EnglishAndMathsEarningsProfileUpdatedEvent englishAndMathsEarningsProfileUpdatedEvent)
    {
        EnglishAndMathsEarningsProfileUpdatedEvent = englishAndMathsEarningsProfileUpdatedEvent;
    }

    public EnglishAndMathsEarningsProfileUpdatedEvent EnglishAndMathsEarningsProfileUpdatedEvent { get; }
}
