using SFA.DAS.Funding.ApprenticeshipEarnings.AcceptanceTests.Constants;
using SFA.DAS.Funding.ApprenticeshipEarnings.Types;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.AcceptanceTests.Extensions;

public static class EarningsGeneratedEventExpectationExtensions
{
    public static bool EventMatchesExpectation(this EarningsGeneratedEvent earningsGeneratedEvent, string expectedUln, decimal expectedDeliveryPeriodLearningAmount)
    {
        return earningsGeneratedEvent.DeliveryPeriods.Count == ApprenticeshipCreatedEventDefaults.ExpectedDeliveryPeriodCount
               && earningsGeneratedEvent.DeliveryPeriods.All(x => x.LearningAmount == expectedDeliveryPeriodLearningAmount
                                                                  && earningsGeneratedEvent.Uln == expectedUln);
    }

    public static bool EventMatchesExpectation(this EarningsGeneratedEvent earningsGeneratedEvent, string expectedUln, string expectedFundingLineType)
    {
        return earningsGeneratedEvent.DeliveryPeriods.All(y => y.FundingLineType == expectedFundingLineType) &&
               earningsGeneratedEvent.Uln == expectedUln &&
               earningsGeneratedEvent.EarningsProfileId != Guid.Empty;
    }
}