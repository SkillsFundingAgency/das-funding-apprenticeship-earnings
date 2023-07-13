using SFA.DAS.Funding.ApprenticeshipEarnings.Domain;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Apprenticeship;
using SFA.DAS.Funding.ApprenticeshipEarnings.Types;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.Command;

public interface IEarningsGeneratedEventBuilder
{
    EarningsGeneratedEvent Build(Apprenticeship apprenticeship);
}

public class EarningsGeneratedEventBuilder : IEarningsGeneratedEventBuilder
{
    public EarningsGeneratedEvent Build(Apprenticeship apprenticeship)
    {
        return new EarningsGeneratedEvent
        {
            ApprenticeshipKey = apprenticeship.ApprenticeshipKey,
            Uln = apprenticeship.Uln,
            EmployerId = apprenticeship.EmployerAccountId,
            ProviderId = apprenticeship.UKPRN,
            TransferSenderEmployerId = apprenticeship.FundingEmployerAccountId,
            AgreedPrice = apprenticeship.AgreedPrice,
            StartDate = apprenticeship.ActualStartDate,
            TrainingCode = apprenticeship.TrainingCode,
            EmployerType = apprenticeship.FundingType.ToOutboundEventEmployerType(),
            DeliveryPeriods = BuildDeliveryPeriods(apprenticeship.EarningsProfile, apprenticeship.FundingLineType),
            EmployerAccountId = apprenticeship.EmployerAccountId,
            PlannedEndDate = apprenticeship.PlannedEndDate
        };
    }

    private static List<DeliveryPeriod> BuildDeliveryPeriods(EarningsProfile earningsProfile, string fundingLineType)
    {
        return earningsProfile.Instalments.Select(instalment => new DeliveryPeriod
            {
                Period = instalment.DeliveryPeriod,
                CalendarMonth = instalment.DeliveryPeriod.ToCalendarMonth(),
                CalenderYear = instalment.AcademicYear.ToCalendarYear(instalment.DeliveryPeriod),
                AcademicYear = instalment.AcademicYear,
                LearningAmount = instalment.Amount,
                FundingLineType = fundingLineType
            })
            .ToList();
    }
}