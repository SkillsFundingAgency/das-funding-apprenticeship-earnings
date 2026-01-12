using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Apprenticeship;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.Domain.ApprenticeshipFunding;

//todo this class superceeded by the new entity version of PeriodInLearning, PriceInPeriod kept in its own file as we use that when matching prices to a period
//public class PeriodInLearning
//{
//    public DateTime StartDate { get; set; }
//    public DateTime EndDate { get; set; }
//    public DateTime OriginalExpectedEndDate => Prices.Max(p => p.OriginalExpectedEndDate);
//    public List<PriceInPeriod> Prices { get; set; }
//}

//public static class PeriodInLearningExtensions
//{

//    public static PeriodInLearning CreatePeriodFromPrice(this IEnumerable<Price> prices)
//    {
//        var startDate = prices.Min(p => p.StartDate);
//        var endDate = prices.Max(p => p.EndDate);

//        return CreatePeriodFromPrices(prices, startDate, endDate, endDate);
//    }

//    public static PeriodInLearning CreatePeriodFromPrices(this IEnumerable<Price> prices, DateTime startDate, DateTime endDate, DateTime originalExpectedEndDate)
//    {
//        if (prices == null || !prices.Any())
//        {
//            throw new ArgumentException("Prices list cannot be null or empty", nameof(prices));
//        }

//        var pricesInPeriod = prices
//            .Select(p => new PriceInPeriod(p, startDate, endDate, p.EndDate))
//            .OrderBy(p => p.StartDate)
//            .ToList();

//        pricesInPeriod.Last().OriginalExpectedEndDate = originalExpectedEndDate;

//        return new PeriodInLearning
//        {
//            StartDate = startDate,
//            EndDate = endDate,
//            Prices = pricesInPeriod
//        };

//    }
//}