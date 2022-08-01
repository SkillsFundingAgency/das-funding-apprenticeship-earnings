using SFA.DAS.Apprenticeships.Events;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.Domain;

public interface IAdjustedPriceCalculator
{
    public decimal CalculateAdjustedPrice(ApprenticeshipCreatedEvent apprenticeshipCreatedEvent);
}

public class AdjustedPriceCalculator : IAdjustedPriceCalculator
{
    public decimal CalculateAdjustedPrice(ApprenticeshipCreatedEvent apprenticeshipCreatedEvent)
    {
        return Math.Min(apprenticeshipCreatedEvent.FundingBandMaximum, apprenticeshipCreatedEvent.AgreedPrice);
    }
}