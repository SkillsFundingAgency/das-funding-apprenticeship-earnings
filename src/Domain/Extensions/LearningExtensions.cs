using SFA.DAS.Funding.ApprenticeshipEarnings.DataAccess.Entities.Apprenticeship;
using SFA.DAS.Funding.ApprenticeshipEarnings.DataAccess.Entities.ShortCourse;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Models.Apprenticeship;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Models.ShortCourse;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Services;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Extensions;

public static class LearningExtensions
{
    public static ApprenticeshipEpisode GetCurrentEpisode(this ApprenticeshipLearning learning, DateTime searchDate)
    {
        var episode = learning.Episodes.FirstOrDefault(x => x.Prices != null && x.Prices.Any(price => price.StartDate <= searchDate && price.EndDate >= searchDate));

        if (episode == null)
        {
            // if no episode is active for the current date, then there could be an episode for the apprenticeship that is yet to start
            episode = learning.Episodes.SingleOrDefault(x => x.Prices != null && x.Prices.Any(price => price.StartDate >= searchDate));
        }

        if (episode == null)
        {
            // if no episode is active for the current date or future, then there could be an episode for the apprenticeship that has finished
            episode = learning.Episodes.Where(x => x.Prices != null).OrderByDescending(x => x.Prices!.Select(y => y.EndDate)).First();
        }

        if (episode == null)
            throw new InvalidOperationException("No current episode found");

        return episode!;
    }

    public static ApprenticeshipEpisode GetCurrentEpisode(this ApprenticeshipLearning learning, ISystemClockService systemClock)
    {
        return learning.GetCurrentEpisode(systemClock.UtcNow.DateTime);
    }

    public static ApprenticeshipEpisode GetApprenticeshipEpisodeFromEntity(this ApprenticeshipLearning learning, ApprenticeshipEpisodeEntity entity)
    {
        return ApprenticeshipEpisode.Get(learning, entity);
    }

    public static ShortCourseEpisode GetShortCourseEpisodeFromEntity(this ShortCourseLearning learning, ShortCourseEpisodeEntity entity)
    {
        return ShortCourseEpisode.Get(learning, entity);
    }

    public static T ToDtoResponse<T>(this ShortCourseLearning learning) where T : DataTransferObjects.ShortCourseEarnings, new()
    {
        var episode = learning.GetEpisode();
        var earningsProfile = episode.EarningsProfile!;

        var response = new T
        {
            EarningProfileVersion = earningsProfile.Version,
            Instalments = earningsProfile.Instalments.Select(i => MapToInstalment(episode, i)).ToList()
        };

        return response;
    }

    private static DataTransferObjects.ShortCourseInstalment MapToInstalment(ShortCourseEpisode episode, ShortCourseInstalment instalment)
    {
        var instalmentType = instalment.Type.ToString();
        var milestoneFlag = Enum.Parse<MilestoneFlags>(instalmentType);

        return new DataTransferObjects.ShortCourseInstalment
        {
            Amount = instalment.Amount,
            CollectionYear = instalment.AcademicYear,
            CollectionPeriod = instalment.DeliveryPeriod,
            Type = instalmentType,
            IsPayable = instalment.IsPayable
        };
    }
}