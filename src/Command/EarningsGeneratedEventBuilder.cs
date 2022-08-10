using SFA.DAS.Funding.ApprenticeshipEarnings.Domain;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Apprenticeship;
using SFA.DAS.Apprenticeships.Types;
using SFA.DAS.Funding.ApprenticeshipEarnings.Types;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.Command;

public interface IEarningsGeneratedEventBuilder
{
    EarningsGeneratedEvent Build(Apprenticeship apprenticeship);
}

public class EarningsGeneratedEventBuilder : IEarningsGeneratedEventBuilder
{
    public EarningsGeneratedEvent Build(Apprenticeship apprenticeshipLearnerEvent)
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
                    StartDate = apprenticeshipLearnerEvent.ActualStartDate,
                    TrainingCode = apprenticeshipLearnerEvent.TrainingCode,
                    EmployerType = apprenticeshipLearnerEvent.FundingType.ToOutboundEventEmployerType(),
                    DeliveryPeriods = BuildDeliveryPeriods(apprenticeshipLearnerEvent.EarningsProfile)
                }
            }
        };
    }

    private static List<DeliveryPeriod> BuildDeliveryPeriods(EarningsProfile earningsProfile)
    {
        return earningsProfile.Instalments.Select(instalment => new DeliveryPeriod
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