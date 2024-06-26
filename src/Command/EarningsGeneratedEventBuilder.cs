using Microsoft.Extensions.Internal;
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
    private ISystemClock _systemClock;

    public EarningsGeneratedEventBuilder(ISystemClock systemClock)
    {
        _systemClock = systemClock;
    }

    public EarningsGeneratedEvent Build(Apprenticeship apprenticeship)
    {
        var currentEpisode = apprenticeship.ApprenticeshipEpisodes.FirstOrDefault(); // DO NOT COMMIT THIS LINE, NEED TO RESOLVE CURRENT EPISODE BASED ON DATE

        return new EarningsGeneratedEvent
        {
            ApprenticeshipKey = apprenticeship.ApprenticeshipKey,
            Uln = apprenticeship.Uln,
            EmployerId = currentEpisode.EmployerAccountId,
            ProviderId = currentEpisode.UKPRN,
            TransferSenderEmployerId = apprenticeship.FundingEmployerAccountId,
            AgreedPrice = apprenticeship.AgreedPrice,
            StartDate = apprenticeship.ActualStartDate,
            TrainingCode = apprenticeship.TrainingCode,
            EmployerType = apprenticeship.FundingType.ToOutboundEventEmployerType(),
            DeliveryPeriods = apprenticeship.BuildDeliveryPeriods() ?? throw new ArgumentException("DeliveryPeriods"),
            EmployerAccountId = currentEpisode.EmployerAccountId,
            PlannedEndDate = apprenticeship.PlannedEndDate,
            ApprovalsApprenticeshipId = apprenticeship.ApprovalsApprenticeshipId,
            EarningsProfileId = apprenticeship.EarningsProfile.EarningsProfileId
        };
    }
}