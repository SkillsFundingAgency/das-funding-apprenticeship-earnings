using SFA.DAS.Funding.ApprenticeshipEarnings.Events;
using SFA.DAS.Funding.ApprenticeshipEarnings.InternalEvents;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.Domain;

public interface IEarningsGeneratedEventBuilder
{
    EarningsGeneratedEvent Build(InternalApprenticeshipLearnerEvent apprenticeshipLearnerEvent, EarningsProfile earningsProfile);
}

public class EarningsGeneratedEventBuilder : IEarningsGeneratedEventBuilder
{
    public EarningsGeneratedEvent Build(InternalApprenticeshipLearnerEvent apprenticeshipLearnerEvent, EarningsProfile earningsProfile)
    {
        return new EarningsGeneratedEvent
        {
            ApprenticeshipKey = apprenticeshipLearnerEvent.ApprenticeshipKey,
            FundingPeriods = new List<FundingPeriod>
            {
                new FundingPeriod()
                {
                    Uln = apprenticeshipLearnerEvent.Uln,
                    CommitmentId = apprenticeshipLearnerEvent.CommitmentId,
                    EmployerId = apprenticeshipLearnerEvent.EmployerId,
                    ProviderId = apprenticeshipLearnerEvent.ProviderId,
                    TransferSenderEmployerId = apprenticeshipLearnerEvent.TransferSenderEmployerId,
                    AgreedPrice = apprenticeshipLearnerEvent.AgreedPrice,
                    StartDate = apprenticeshipLearnerEvent.ActualStartDate,
                    TrainingCode = apprenticeshipLearnerEvent.TrainingCode,
                    EmployerType = apprenticeshipLearnerEvent.EmployerType.ToOutboundEventEmployerType(),
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