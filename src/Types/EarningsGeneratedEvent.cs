namespace SFA.DAS.Funding.ApprenticeshipEarnings.Types;

public class EarningsGeneratedEvent
{
    public Guid ApprenticeshipKey { get; set; }
    public List<FundingPeriod> FundingPeriods { get; set; }
}