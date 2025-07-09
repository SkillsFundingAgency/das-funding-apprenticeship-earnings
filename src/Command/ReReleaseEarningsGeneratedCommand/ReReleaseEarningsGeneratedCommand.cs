namespace SFA.DAS.Funding.ApprenticeshipEarnings.Command.ReReleaseEarningsGeneratedCommand;

public class ReReleaseEarningsGeneratedCommand(long ukprn) : ICommand
{
    public long Ukprn { get; } = ukprn;
}
