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
        var currentEpisode = apprenticeship.GetCurrentEpisode(_systemClock);

        return new EarningsGeneratedEvent
        {
            ApprenticeshipKey = apprenticeship.ApprenticeshipKey,
            Uln = apprenticeship.Uln,
            EmployerId = currentEpisode.EmployerAccountId,
            ProviderId = currentEpisode.UKPRN,
            TransferSenderEmployerId = apprenticeship.FundingEmployerAccountId,
            AgreedPrice = currentEpisode.AgreedPrice,
            StartDate = currentEpisode.ActualStartDate,
            TrainingCode = currentEpisode.TrainingCode,
            EmployerType = apprenticeship.FundingType.ToOutboundEventEmployerType(),
            DeliveryPeriods = apprenticeship.BuildDeliveryPeriods() ?? throw new ArgumentException("DeliveryPeriods"),
            EmployerAccountId = currentEpisode.EmployerAccountId,
            PlannedEndDate = currentEpisode.PlannedEndDate,
            ApprovalsApprenticeshipId = apprenticeship.ApprovalsApprenticeshipId,
            EarningsProfileId = apprenticeship.EarningsProfile.EarningsProfileId
        };
    }
}