using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Apprenticeship;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Factories;

public interface IApprenticeshipFactory
{
    Apprenticeship.Apprenticeship CreateNew(long approvalsApprenticeshipId, string uln, List<ApprenticeshipEpisode> apprenticeshipEpisodes);
}
