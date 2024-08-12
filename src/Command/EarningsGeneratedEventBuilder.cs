using SFA.DAS.Funding.ApprenticeshipEarnings.Domain;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Apprenticeship;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Services;
using SFA.DAS.Funding.ApprenticeshipEarnings.Types;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.Command;

public interface IEarningsGeneratedEventBuilder
{
    EarningsGeneratedEvent Build(Apprenticeship apprenticeship);
}

public class EarningsGeneratedEventBuilder : IEarningsGeneratedEventBuilder
{
    private readonly ISystemClockService _systemClock;

    public EarningsGeneratedEventBuilder(ISystemClockService systemClock)
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
            TransferSenderEmployerId = currentEpisode.FundingEmployerAccountId,
            AgreedPrice = currentEpisode.Prices![0].AgreedPrice,
            StartDate = currentEpisode.Prices![0].StartDate,
            TrainingCode = currentEpisode.TrainingCode,
            EmployerType = currentEpisode.FundingType.ToOutboundEventEmployerType(),
            DeliveryPeriods = currentEpisode.BuildDeliveryPeriods() ?? throw new ArgumentException("DeliveryPeriods"),
            EmployerAccountId = currentEpisode.EmployerAccountId,
            PlannedEndDate = currentEpisode.Prices![0].EndDate,
            ApprovalsApprenticeshipId = apprenticeship.ApprovalsApprenticeshipId,
            EarningsProfileId = currentEpisode.EarningsProfile!.EarningsProfileId
        };
    }
}