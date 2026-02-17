using SFA.DAS.Funding.ApprenticeshipEarnings.DataAccess.Entities;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.ApprenticeshipFunding;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Extensions;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Services;
using SFA.DAS.Funding.ApprenticeshipEarnings.Types;
using SFA.DAS.Learning.Types;

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
        Guid episodeKey,
        bool isApproved)
    {
        return new EarningsProfile(onProgramTotal, instalments, additionalPayments, mathsAndEnglishCourses,
            completionPayment, episodeKey, isApproved, episode.AddChildToRoot);
    }

    public static List<DeliveryPeriod> BuildDeliveryPeriods(this ApprenticeshipEpisode currentEpisode)
    {
        var deliveryPeriods = new List<DeliveryPeriod>();

        if (currentEpisode.EarningsProfile != null)
        {
            deliveryPeriods.AddRange(currentEpisode.EarningsProfile.Instalments.Select(instalment => new DeliveryPeriod
            (
                instalment.DeliveryPeriod.ToCalendarMonth(),
                instalment.AcademicYear.ToCalendarYear(instalment.DeliveryPeriod),
                instalment.DeliveryPeriod,
                instalment.AcademicYear,
                instalment.Amount,
                currentEpisode.FundingLineType,
                InstalmentTypes.OnProgramme
            )));

            deliveryPeriods.AddRange(currentEpisode.EarningsProfile.AdditionalPayments.Select(additionalPayment => new DeliveryPeriod(
                additionalPayment.DeliveryPeriod.ToCalendarMonth(),
                additionalPayment.AcademicYear.ToCalendarYear(additionalPayment.DeliveryPeriod),
                additionalPayment.DeliveryPeriod,
                additionalPayment.AcademicYear,
                additionalPayment.Amount,
                currentEpisode.FundingLineType,
                additionalPayment.AdditionalPaymentType)));

            deliveryPeriods.AddRange(currentEpisode.EarningsProfile.MathsAndEnglishCourses.SelectMany(x => x.Instalments).Select(mathsAndEnglishInstalment => new DeliveryPeriod(
                mathsAndEnglishInstalment.DeliveryPeriod.ToCalendarMonth(),
                mathsAndEnglishInstalment.AcademicYear.ToCalendarYear(mathsAndEnglishInstalment.DeliveryPeriod),
                mathsAndEnglishInstalment.DeliveryPeriod,
                mathsAndEnglishInstalment.AcademicYear,
                mathsAndEnglishInstalment.Amount,
                currentEpisode.FundingLineType,
                InstalmentTypes.MathsAndEnglish)));
        }

        return deliveryPeriods;
    }

    internal static DateTime? GetLastDayOfLearning(this ApprenticeshipEpisode episode)
    {
        var plausibleLastDaysOfLearning = new List<DateTime?>()
        {
            episode.CompletionDate,
            episode.WithdrawalDate,
            episode.PauseDate
        };

        return plausibleLastDaysOfLearning
            .Where(d => d.HasValue)
            .OrderBy(d => d.Value)
            .FirstOrDefault();
    }

    internal static ApprenticeshipEarningsRecalculatedEvent CreateApprenticeshipEarningsRecalculatedEvent(this ApprenticeshipEpisode episode, Apprenticeship apprenticeship)
    {
        return new ApprenticeshipEarningsRecalculatedEvent
        {
            ApprenticeshipKey = apprenticeship.ApprenticeshipKey,
            DeliveryPeriods = episode.BuildDeliveryPeriods() ?? throw new ArgumentException("DeliveryPeriods"),
            EarningsProfileId = episode.EarningsProfile!.EarningsProfileId,
            StartDate = episode.Prices.OrderBy(x => x.StartDate).First().StartDate,
            PlannedEndDate = episode.Prices.OrderBy(x => x.StartDate).Last().EndDate,
            AgeAtStartOfApprenticeship = episode.AgeAtStartOfApprenticeship
        };
    }

    internal static bool PricesAreIdentical(this ApprenticeshipEpisode episode, List<LearningEpisodePrice> prices)
    {
        var existingPrices = episode.Prices;
        if (prices.Count != existingPrices.Count)
            return false;

        foreach (var price in prices)
        {
            var matchingPrice = existingPrices.SingleOrDefault(x => x.PriceKey == price.Key);
            if (matchingPrice == null)
                return false;
            if (matchingPrice.StartDate != price.StartDate || matchingPrice.EndDate != price.EndDate || matchingPrice.AgreedPrice != price.TotalPrice)
                return false;
        }

        return true;
    }
}