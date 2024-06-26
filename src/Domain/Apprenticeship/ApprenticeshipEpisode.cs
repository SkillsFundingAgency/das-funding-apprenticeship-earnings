using SFA.DAS.Funding.ApprenticeshipEarnings.DurableEntities.Models;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Apprenticeship;

public class ApprenticeshipEpisode
{
    public long UKPRN { get; }

    public ApprenticeshipEpisode(ApprenticeshipEpisodeModel apprenticeshipEpisodeModel)
    {
        UKPRN = apprenticeshipEpisodeModel.UKPRN;
    }
}
