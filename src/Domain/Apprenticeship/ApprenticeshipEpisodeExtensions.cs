using SFA.DAS.Funding.ApprenticeshipEarnings.DataAccess.Entities;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.ApprenticeshipFunding;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Services;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Apprenticeship;

public static class LearningEpisodeExtensions
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

    internal static DateTime? GetLastDayOfLearning(this ApprenticeshipEpisode episode)
    {
        var plausibleLastDaysOfLearning = new List<DateTime?>()
        {
            episode.CompletionDate,
            episode.WithdrawalDate
        };

        if (IsPauseDateOutsideBreaks(episode))
        {
            plausibleLastDaysOfLearning.Add(episode.PauseDate);
        }

        return plausibleLastDaysOfLearning
            .Where(d => d.HasValue)
            .OrderBy(d => d.Value)
            .FirstOrDefault();
    }

    internal static List<PeriodInLearning> GetPeriodsInLearning(this ApprenticeshipEpisode episode)
    {
        // If no breaks, return single period from min start to max end
        if (episode.BreaksInLearning == null || !episode.BreaksInLearning.Any())
        {
            return new List<PeriodInLearning>
            {
                episode.Prices.CreatePeriodFromPrice()
            };
        }

        var periods = new List<PeriodInLearning>();
        var sortedBreaks = episode.BreaksInLearning.OrderBy(b => b.StartDate);
        var startDate = episode.Prices.Min(p => p.StartDate);

        foreach (var bil in sortedBreaks)
        {
            // Period ends the day before the break starts
            var endDate = bil.StartDate.AddDays(-1);

            var pricesInPeriod = episode.Prices.Where(p => p.StartDate <= endDate && p.EndDate >= startDate);

            periods.Add(pricesInPeriod.CreatePeriodFromPrices(startDate, endDate, bil.PreviousPeriodExpectedEndDate));

            // Next period starts the day after the break ends
            startDate = bil.EndDate.AddDays(1);
        }

        // Add the final period after the last break
        var pricesInLastPeriod = episode.Prices.Where(p => p.EndDate >= startDate);

        if (pricesInLastPeriod.Any())
        {
            var endDate = pricesInLastPeriod.Max(p => p.EndDate);
            var expectedEndDate = episode.Prices.Max(p => p.EndDate);
            periods.Add(pricesInLastPeriod.CreatePeriodFromPrices(startDate, endDate, expectedEndDate));
        }

        return periods;
    }

    private static bool IsPauseDateOutsideBreaks(ApprenticeshipEpisode episode)
    {
        if (!episode.PauseDate.HasValue || episode.BreaksInLearning == null)
            return true;

        var pauseDate = episode.PauseDate.Value;

        // Pause is inside a break if it is >= Start and <= End
        bool insideBreak = episode.BreaksInLearning.Any(b =>
            pauseDate >= b.StartDate &&
            pauseDate <= b.EndDate);

        return !insideBreak;
    }
}