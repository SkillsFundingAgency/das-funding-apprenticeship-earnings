using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Apprenticeship;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Factories;

public class ApprenticeshipFactory : IApprenticeshipFactory
{
    public Apprenticeship.Apprenticeship CreateNew(long approvalsApprenticeshipId, string uln, List<ApprenticeshipEpisode> apprenticeshipEpisodes)
    {
        return new Apprenticeship.Apprenticeship(approvalsApprenticeshipId, uln, apprenticeshipEpisodes);
    }
}
