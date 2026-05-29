using SFA.DAS.Funding.ApprenticeshipEarnings.DataAccess.Entities.Apprenticeship;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.DataAccess.Entities;

public static class EpisodePriceEntityExtensions
{
    public static ApprenticeshipPeriodInLearningEntity ToSinglePeriodInLearning(this ApprenticeshipEpisodePriceEntity episodePrice)
    {
        return new ApprenticeshipPeriodInLearningEntity
        {
            Key = Guid.NewGuid(),
            StartDate = episodePrice.StartDate,
            EndDate = null,
            OriginalExpectedEndDate = episodePrice.EndDate,
            EpisodeKey = episodePrice.EpisodeKey
        };
    }
}