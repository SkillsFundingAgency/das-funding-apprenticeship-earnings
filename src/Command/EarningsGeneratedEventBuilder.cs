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
            DeliveryPeriods = apprenticeship.BuildDeliveryPeriods(),
            EmployerAccountId = apprenticeship.EmployerAccountId,
            PlannedEndDate = apprenticeship.PlannedEndDate,
            ApprovalsApprenticeshipId = apprenticeship.ApprovalsApprenticeshipId
        };
    }
}