using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Apprenticeship;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.ApprenticeshipFunding;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Extensions;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Calculations;

public static class OnProgramPayments
{
    public static List<OnProgramPayment> GenerateEarningsForEpisodePrices(IEnumerable<PeriodInLearning> periodsInLearning, decimal fundingBandMaximum, out decimal onProgramTotal, out decimal completionPayment)
    {
        var onProgramPayments = new List<OnProgramPayment>();
        var runningTotal = 0m;
        completionPayment = 0;

        foreach (var periodInLearning in periodsInLearning)
        {
            var paymentsForPeriod = GenerateForLearningPeriod(periodInLearning, runningTotal, fundingBandMaximum, out var periodOnProgramTotal, out var periodCompletionPayment);
            runningTotal += paymentsForPeriod.Sum(x=>x.Amount);
            completionPayment = periodCompletionPayment;

            onProgramPayments.AddRange(paymentsForPeriod);
        }

        onProgramTotal = runningTotal;

        return onProgramPayments;
    }

    public static List<Instalment> SoftDeleteAfterLastDayOfLearning(List<Instalment> instalments, List<Price> prices, DateTime lastDayOfLearning)
    {
        List<Instalment> result;

        var academicYear = lastDayOfLearning.ToAcademicYear();
        var deliveryPeriod = lastDayOfLearning.ToDeliveryPeriod();

        var startDate = prices.Min(x => x.StartDate);
        var qualifyingPeriodDays = QualifyingPeriod.GetQualifyingPeriodDays(startDate, prices.Max(x => x.EndDate));
        var qualifyingDate = startDate.AddDays(qualifyingPeriodDays - 1); //With shorter apprenticeships, this qualifying period will change
        if (lastDayOfLearning < qualifyingDate)
        {
            result = instalments.Where(x =>
                    x.AcademicYear < academicYear) //keep earnings from previous academic years
                .ToList();
        }
        else
        {
            result = instalments.Where(x =>
                    x.AcademicYear < academicYear //keep earnings from previous academic years
            || x.AcademicYear == academicYear && x.DeliveryPeriod < deliveryPeriod //keep earnings from previous delivery periods in the same academic year
            || x.AcademicYear == academicYear && x.DeliveryPeriod == deliveryPeriod && lastDayOfLearning.Day == DateTime.DaysInMonth(lastDayOfLearning.Year, lastDayOfLearning.Month) //keep earnings in the last delivery period of learning if the learner is in learning on the census date
            || x.AcademicYear == academicYear && x.DeliveryPeriod == deliveryPeriod && (x.Type == InstalmentType.Balancing || x.Type == InstalmentType.Completion)) //keep earnings in the last delivery period of learning if they are balancing or completion payments
                .ToList();
        }

        instalments.ForEach(i =>
        {
            if (!result.Contains(i))
            {
                i.SoftDelete();
            }
        });

        return instalments;
    }

    private static List<OnProgramPayment> GenerateForLearningPeriod(PeriodInLearning periodInLearning, decimal paidInPreviousPeriods, decimal fundingBandMaximum, out decimal onProgramTotal, out decimal completionPayment)
    {
        var onProgramPayments = new List<OnProgramPayment>();
        onProgramTotal = 0;
        completionPayment = 0;

        var orderedPrices = periodInLearning.Prices.OrderBy(x => x.StartDate).ToList();
        var apprenticeshipEndDate = periodInLearning.OriginalExpectedEndDate;
        var runningTotal = paidInPreviousPeriods;

        foreach (var price in orderedPrices)
        {
            var apprenticeshipFunding = new ApprenticeshipFunding.ApprenticeshipFunding(
                price.AgreedPrice,
                fundingBandMaximum);

            onProgramTotal = apprenticeshipFunding.OnProgramTotal;
            completionPayment = apprenticeshipFunding.CompletionPayment;

            var remainingCostToDistribute = onProgramTotal - runningTotal;
            var earningsForPricePeriod = GenerateEarningsForPeriod(remainingCostToDistribute, price, apprenticeshipEndDate);

            onProgramPayments.AddRange(earningsForPricePeriod);
            runningTotal += earningsForPricePeriod.Sum(x => x.Amount);
        }

        return onProgramPayments;
    }

    private static IEnumerable<OnProgramPayment> GenerateEarningsForPeriod(decimal total, PriceInPeriod priceInPeriod, DateTime apprenticeshipEndDate)
    {
        var periodInstalmentCount = CalculateInstalmentCount(priceInPeriod.StartDate, priceInPeriod.EndDate);
        var remainingInstalmentCount = CalculateInstalmentCount(priceInPeriod.StartDate, apprenticeshipEndDate);
        var instalmentAmount = decimal.Round(total / remainingInstalmentCount, 5);

        var onProgramPayments = new List<OnProgramPayment>();
        var currentMonth = new DateTime(priceInPeriod.StartDate.Year, priceInPeriod.StartDate.Month, 1, 0, 0, 0, DateTimeKind.Utc);

        for (var i = 0; i < periodInstalmentCount; i++)
        {

            var earning = new OnProgramPayment
            {
                PriceKey = priceInPeriod.PriceKey,
                DeliveryPeriod = currentMonth.ToDeliveryPeriod(),
                AcademicYear = currentMonth.ToAcademicYear(),
                Amount = instalmentAmount
            };

            onProgramPayments.Add(earning);
            currentMonth = currentMonth.AddMonths(1);
        }

        return onProgramPayments;
    }

    private static int CalculateInstalmentCount(DateTime startDate, DateTime endDate)
    {
        var startDateMonth = new DateTime(startDate.Year, startDate.Month, 1, 0, 0, 0, DateTimeKind.Utc);
        var endDateMonth = GetLastPaymentMonth(endDate);
        var instalmentCount = (endDateMonth.Year - startDateMonth.Year) * 12 +
            endDateMonth.Month -
            startDateMonth.Month + 1;
        return instalmentCount;
    }

    private static DateTime GetLastPaymentMonth(DateTime endDate)
    {
        var isLastDayOfMonth = endDate.Day == DateTime.DaysInMonth(endDate.Year, endDate.Month);
        return isLastDayOfMonth
            ? new DateTime(endDate.Year, endDate.Month, 1, 0, 0, 0, DateTimeKind.Utc)
            : new DateTime(endDate.Year, endDate.Month, 1, 0, 0, 0, DateTimeKind.Utc).AddMonths(-1);
    }
}
