using SFA.DAS.Apprenticeships.Types;
using SFA.DAS.Funding.ApprenticeshipEarnings.Types;

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
                new()
                {
                    Uln = long.Parse(apprenticeshipLearnerEvent.Uln),
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

    private static List<DeliveryPeriod> BuildDeliveryPeriods(EarningsProfile earningsProfile)
    {
        return earningsProfile.Installments.Select(instalment => new DeliveryPeriod
            {
                Period = instalment.DeliveryPeriod,
                CalendarMonth = instalment.DeliveryPeriod.ToCalendarMonth(),
                CalenderYear = instalment.AcademicYear.ToCalendarYear(instalment.DeliveryPeriod),
                AcademicYear = instalment.AcademicYear,
                LearningAmount = instalment.Amount
            })
            .ToList();
    }
}