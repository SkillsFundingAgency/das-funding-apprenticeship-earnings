using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Extensions;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Models.Apprenticeship;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Services;
using SFA.DAS.Funding.ApprenticeshipEarnings.Types;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.Command;

public interface IEarningsGeneratedEventBuilder
{
    EarningsGeneratedEvent Build(ApprenticeshipLearning learning);
}

public class EarningsGeneratedEventBuilder : IEarningsGeneratedEventBuilder
{
    private readonly ISystemClockService _systemClock;

    public EarningsGeneratedEventBuilder(ISystemClockService systemClock)
    {
        _systemClock = systemClock;
    }

    public EarningsGeneratedEvent Build(ApprenticeshipLearning learning)
    {
        var currentEpisode = learning.GetCurrentEpisode(_systemClock);

        var result = new EarningsGeneratedEvent
        {
            ApprenticeshipKey = learning.LearningKey,
            Uln = learning.Uln,
            EmployerId = currentEpisode.EmployerAccountId,
            ProviderId = currentEpisode.UKPRN,
            TransferSenderEmployerId = currentEpisode.FundingEmployerAccountId,
            AgreedPrice = currentEpisode.Prices!.First().AgreedPrice,
            StartDate = currentEpisode.Prices!.First().StartDate,
            TrainingCode = currentEpisode.TrainingCode,
            EmployerType = currentEpisode.FundingType.ToOutboundEventEmployerType(),
            DeliveryPeriods = currentEpisode.BuildDeliveryPeriods() ?? throw new ArgumentException("DeliveryPeriods"),
            EmployerAccountId = currentEpisode.EmployerAccountId,
            PlannedEndDate = currentEpisode.Prices!.First().EndDate,
            ApprovalsApprenticeshipId = learning.ApprovalsApprenticeshipId,
            EarningsProfileId = currentEpisode.EarningsProfile!.EarningsProfileId,
            AgeAtStartOfLearning = currentEpisode.AgeAtStartOfApprenticeship
        };

        return result;
    }
}