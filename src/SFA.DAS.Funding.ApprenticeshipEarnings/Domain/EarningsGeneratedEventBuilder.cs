using SFA.DAS.Apprenticeships.Events;
using SFA.DAS.Funding.ApprenticeshipEarnings.Events;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.Domain;

public interface IEarningsGeneratedEventBuilder
{
    EarningsGeneratedEvent Build(ApprenticeshipCreatedEvent apprenticeshipLearnerEvent, EarningsProfile earningsProfile);
}

public class EarningsGeneratedEventBuilder : IEarningsGeneratedEventBuilder
{
    public EarningsGeneratedEvent Build(ApprenticeshipCreatedEvent apprenticeshipLearnerEvent, EarningsProfile earningsProfile)
    {
        return new EarningsGeneratedEvent
        {
            ApprenticeshipKey = apprenticeshipLearnerEvent.ApprenticeshipKey,
            FundingPeriods = new List<FundingPeriod>
            {
                new FundingPeriod()
                {
                    Uln = apprenticeshipLearnerEvent.Uln,
                    EmployerId = apprenticeshipLearnerEvent.EmployerAccountId,
                    ProviderId = apprenticeshipLearnerEvent.UKPRN,
                    TransferSenderEmployerId = apprenticeshipLearnerEvent.FundingEmployerAccountId,
                    AgreedPrice = apprenticeshipLearnerEvent.AgreedPrice,
                    StartDate = apprenticeshipLearnerEvent.ActualStartDate.GetValueOrDefault(),
                    TrainingCode = apprenticeshipLearnerEvent.TrainingCode,
                    EmployerType = apprenticeshipLearnerEvent.FundingType.ToOutboundEventEmployerType(),
                    DeliveryPeriods = BuildDeliveryPeriods(earningsProfile)
                }
            }
        };
    }

    private List<DeliveryPeriod> BuildDeliveryPeriods(EarningsProfile earningsProfile)
    {
        var deliveryPeriods = new List<DeliveryPeriod>();
        foreach (var installment in earningsProfile.Installments)
        {
            deliveryPeriods.Add(new DeliveryPeriod
            {
                Period = installment.DeliveryPeriod,
                CalendarMonth = installment.DeliveryPeriod.ToCalendarMonth(),
                CalenderYear = installment.AcademicYear.ToCalendarYear(installment.DeliveryPeriod),
                AcademicYear = installment.AcademicYear,
                LearningAmount = installment.Amount
            });
        }

        return deliveryPeriods;
    }
}