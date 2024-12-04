namespace SFA.DAS.Funding.ApprenticeshipEarnings.Command.ReReleaseEarningsGeneratedCommand;

public class ReReleaseEarningsGeneratedCommand : ICommand
{
    public long Ukprn { get; }

    public ReReleaseEarningsGeneratedCommand(long ukprn)
    {
        Ukprn = ukprn;
    }
}
