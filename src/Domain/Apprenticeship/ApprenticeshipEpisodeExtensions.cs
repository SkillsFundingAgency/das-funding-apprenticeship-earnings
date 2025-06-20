using SFA.DAS.Funding.ApprenticeshipEarnings.DataAccess.Entities;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Services;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Apprenticeship;

public static class ApprenticeshipEpisodeExtensions
{
    public static Price GetCurrentPrice(this ApprenticeshipEpisode episode, ISystemClockService systemClock)
    {
        var price = episode?.Prices?.FirstOrDefault(x => x.StartDate <= systemClock.UtcNow && x.EndDate >= systemClock.UtcNow);

        if (price == null)
        {
            price = episode?.Prices?.FirstOrDefault(x => x.StartDate >= systemClock.UtcNow);
        }

        if(price == null)
            throw new InvalidOperationException("No current price found");

        return price;
    }

    public static EarningsProfile GetEarningsProfileFromModel(this ApprenticeshipEpisode episode, EarningsProfileModel entity)
    {
        return EarningsProfile.Get(episode, entity);
    }

    public static EarningsProfile CreateEarningsProfile(
        this ApprenticeshipEpisode episode, 
        decimal onProgramTotal,
        List<Instalment> instalments,
        List<AdditionalPayment> additionalPayments,
        List<MathsAndEnglish> mathsAndEnglishCourses,
        decimal completionPayment,
        Guid episodeKey)
    {
        return new EarningsProfile(onProgramTotal, instalments,additionalPayments,mathsAndEnglishCourses,completionPayment,episodeKey, episode.AddChildToRoot);
    }
}