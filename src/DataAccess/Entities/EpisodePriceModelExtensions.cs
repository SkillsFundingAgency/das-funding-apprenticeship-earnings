namespace SFA.DAS.Funding.ApprenticeshipEarnings.DataAccess.Entities;

public static class EpisodePriceModelExtensions
{
    public static EpisodePeriodInLearningModel ToSinglePeriodInLearning(this EpisodePriceModel episodePrice)
    {
        return new EpisodePeriodInLearningModel
        {
            Key = Guid.NewGuid(),
            StartDate = episodePrice.StartDate,
            EndDate = episodePrice.EndDate,
            OriginalExpectedEndDate = episodePrice.EndDate,
            EpisodeKey = episodePrice.EpisodeKey
        };
    }
}