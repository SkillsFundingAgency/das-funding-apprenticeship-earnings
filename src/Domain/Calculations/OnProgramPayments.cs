using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.ApprenticeshipFunding;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Extensions;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Models;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Models.Apprenticeship;
using System.Linq;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Calculations;

public static class OnProgramPayments
{
    public static List<OnProgramPayment> GenerateEarningsForEpisodePrices(List<(ApprenticeshipPeriodInLearning periodInLearning, List<PriceInPeriod> prices)> periodsInLearningWithPrices, decimal fundingBandMaximum, out decimal onProgramTotal, out decimal completionPayment)
    {
        var onProgramPayments = new List<OnProgramPayment>();
        var runningTotal = 0m;
        completionPayment = 0;
        onProgramTotal = 0;

        foreach (var periodInLearningWithPrices in periodsInLearningWithPrices.OrderBy(x => x.periodInLearning.StartDate))
        {
            var paymentsForPeriod = GenerateForLearningPeriod(periodInLearningWithPrices, runningTotal, fundingBandMaximum, out var periodOnProgramTotal, out var periodCompletionPayment);
            runningTotal += paymentsForPeriod.Sum(x=>x.Amount);
            completionPayment = periodCompletionPayment;
            onProgramTotal = periodOnProgramTotal;
            onProgramPayments.AddRange(paymentsForPeriod);
        }

        return onProgramPayments;
    }

    public static List<ApprenticeshipInstalment> RemoveAfterLastDayOfLearning(List<ApprenticeshipInstalment> instalments, IReadOnlyCollection<ApprenticeshipPeriodInLearning> periodsInLearning, DateTime lastDayOfLearning)
    {
        var academicYear = lastDayOfLearning.ToAcademicYear();
        var deliveryPeriod = lastDayOfLearning.ToDeliveryPeriod();

        var nonQualifyingInstalments = instalments.Where(x =>
            x.Type == InstalmentType.Regular &&
            periodsInLearning.Any(p => !p.QualifiesForEarnings(lastDayOfLearning) && p.SpansDeliveryPeriod(x.AcademicYear, x.DeliveryPeriod, lastDayOfLearning))
        ).ToList();

        var postWithdrawalInstalments = instalments.Where(x =>
            !nonQualifyingInstalments.Contains(x) &&
            (x.AcademicYear > academicYear
            || (x.AcademicYear == academicYear && x.DeliveryPeriod > deliveryPeriod)
            || (x.AcademicYear == academicYear && x.DeliveryPeriod == deliveryPeriod
                && lastDayOfLearning.Day < DateTime.DaysInMonth(lastDayOfLearning.Year, lastDayOfLearning.Month)
                && x.Type != InstalmentType.Balancing && x.Type != InstalmentType.Completion))
        ).ToList();

        var survivingInstalments = instalments.Where(x => x.Type == InstalmentType.Regular && !nonQualifyingInstalments.Contains(x) && !postWithdrawalInstalments.Contains(x)).ToList();

        if (survivingInstalments.Any() && nonQualifyingInstalments.Any())
        {
            var amountToRedistribute = nonQualifyingInstalments.Sum(x => x.Amount);
            var amountPerInstalment = decimal.Round(amountToRedistribute / survivingInstalments.Count, 5);
            foreach (var instalment in survivingInstalments)
            {
                instalment.UpdateAmount(instalment.Amount + amountPerInstalment);
            }

            var remainder = amountToRedistribute - (amountPerInstalment * survivingInstalments.Count);
            survivingInstalments.Last().UpdateAmount(survivingInstalments.Last().Amount + remainder);
        }

        instalments.RemoveAll(x => nonQualifyingInstalments.Contains(x) || postWithdrawalInstalments.Contains(x));

        return instalments;
    }

    private static List<OnProgramPayment> GenerateForLearningPeriod((ApprenticeshipPeriodInLearning periodInLearning, List<PriceInPeriod> prices) periodInLearningWithPrices, decimal paidInPreviousPeriods, decimal fundingBandMaximum, out decimal onProgramTotal, out decimal completionPayment)
    {
        var onProgramPayments = new List<OnProgramPayment>();
        onProgramTotal = 0;
        completionPayment = 0;

        var orderedPrices = periodInLearningWithPrices.prices.OrderBy(x => x.StartDate).ToList();
        var apprenticeshipEndDate = periodInLearningWithPrices.periodInLearning.OriginalExpectedEndDate;
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

        if (remainingInstalmentCount == 0 || periodInstalmentCount == 0)
        {
            return new List<OnProgramPayment>();
        }

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
