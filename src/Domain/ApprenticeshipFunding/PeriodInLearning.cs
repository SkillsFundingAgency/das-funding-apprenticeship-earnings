using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Apprenticeship;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Calculations;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.Domain.ApprenticeshipFunding;

public class PeriodInLearning
{
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public DateTime OriginalExpectedEndDate => Prices.Max(p => p.OriginalExpectedEndDate);
    public List<PriceInPeriod> Prices { get; set; }
    public bool HasReachedQualificationPeriod => EndDate >= QualifyingDate;
    public int QualifyingPeriodDays => QualifyingPeriod.GetQualifyingPeriodDays(StartDate, OriginalExpectedEndDate);
    public DateTime QualifyingDate => StartDate.AddDays(QualifyingPeriodDays-1);

}

public class PriceInPeriod
{
    public DateTime StartDate { get; private set; }
    public DateTime EndDate { get; private set; }
    public decimal AgreedPrice { get; private set; }
    public Guid PriceKey { get; private set; }
    public DateTime OriginalExpectedEndDate { get; set; }

    public PriceInPeriod(Price price, DateTime periodStartDate, DateTime periodEndDate, DateTime originalExpectedEndDate)
    {
        StartDate = price.StartDate > periodStartDate ? price.StartDate : periodStartDate;
        EndDate = price.EndDate < periodEndDate ? price.EndDate : periodEndDate;
        AgreedPrice = price.AgreedPrice;
        PriceKey = price.PriceKey;
        OriginalExpectedEndDate = originalExpectedEndDate;
    }
}

public static class PeriodInLearningExtensions
{

    public static PeriodInLearning CreatePeriodFromPrice(this IEnumerable<Price> prices)
    {
        var startDate = prices.Min(p => p.StartDate);
        var endDate = prices.Max(p => p.EndDate);

        return CreatePeriodFromPrices(prices, startDate, endDate, endDate);
    }

    public static PeriodInLearning CreatePeriodFromPrices(this IEnumerable<Price> prices, DateTime startDate, DateTime endDate, DateTime originalExpectedEndDate)
    {
        if (prices == null || !prices.Any())
        {
            throw new ArgumentException("Prices list cannot be null or empty", nameof(prices));
        }

        var pricesInPeriod = prices
            .Select(p => new PriceInPeriod(p, startDate, endDate, p.EndDate))
            .OrderBy(p => p.StartDate)
            .ToList();

        pricesInPeriod.Last().OriginalExpectedEndDate = originalExpectedEndDate;

        return new PeriodInLearning
        {
            StartDate = startDate,
            EndDate = endDate,
            Prices = pricesInPeriod
        };

    }
}