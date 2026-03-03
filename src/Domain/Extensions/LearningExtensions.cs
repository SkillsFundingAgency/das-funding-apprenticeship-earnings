using SFA.DAS.Funding.ApprenticeshipEarnings.DataAccess.Entities.Apprenticeship;
using SFA.DAS.Funding.ApprenticeshipEarnings.DataAccess.Entities.ShortCourse;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Models.Apprenticeship;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Models.ShortCourse;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Services;
using LearningDomainModel = SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Models.Learning;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Extensions;

public static class LearningExtensions
{
    public static ApprenticeshipEpisode GetCurrentEpisode(this LearningDomainModel learning, DateTime searchDate)
    {
        var episode = learning.ApprenticeshipEpisodes.FirstOrDefault(x => x.Prices != null && x.Prices.Any(price => price.StartDate <= searchDate && price.EndDate >= searchDate));

        if (episode == null)
        {
            // if no episode is active for the current date, then there could be an episode for the apprenticeship that is yet to start
            episode = learning.ApprenticeshipEpisodes.SingleOrDefault(x => x.Prices != null && x.Prices.Any(price => price.StartDate >= searchDate));
        }

        if (episode == null)
        {
            // if no episode is active for the current date or future, then there could be an episode for the apprenticeship that has finished
            episode = learning.ApprenticeshipEpisodes.Where(x => x.Prices != null).OrderByDescending(x => x.Prices!.Select(y => y.EndDate)).First();
        }

        if (episode == null)
            throw new InvalidOperationException("No current episode found");

        return episode!;
    }

    public static ApprenticeshipEpisode GetCurrentEpisode(this LearningDomainModel learning, ISystemClockService systemClock)
    {
        return learning.GetCurrentEpisode(systemClock.UtcNow.DateTime);
    }

    public static ApprenticeshipEpisode GetEpisode(this LearningDomainModel learning,  Guid episodeKey)
    {
        var episode = learning.ApprenticeshipEpisodes.SingleOrDefault(e => e.EpisodeKey == episodeKey);
        if (episode == null)
            throw new InvalidOperationException($"No episode found for key {episodeKey}");
        return episode!;
    }

    public static ApprenticeshipEpisode GetApprenticeshipEpisodeFromEntity(this LearningDomainModel learning, ApprenticeshipEpisodeEntity entity)
    {
        return ApprenticeshipEpisode.Get(learning, entity);
    }

    public static ShortCourseEpisode GetShortCourseEpisodeFromEntity(this LearningDomainModel learning, ShortCourseEpisodeEntity entity)
    {
        return ShortCourseEpisode.Get(learning, entity);
    }
}