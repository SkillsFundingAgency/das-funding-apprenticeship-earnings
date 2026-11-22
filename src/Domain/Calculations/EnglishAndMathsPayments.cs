using SFA.DAS.Funding.ApprenticeshipEarnings.DataAccess.Entities.EnglishAndMaths;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Extensions;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Models.EnglishAndMaths;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Calculations;

public static class EnglishAndMathsPayments
{
    public static List<EnglishAndMathsInstalmentEntity> GenerateInstalments(EnglishAndMaths englishAndMaths, DateTime? apprenticeshipWithdrawalDate = null)
    {
        if (IsInvalidCourse(englishAndMaths))
            return [];

        var endDates = englishAndMaths.GetEffectiveEndDates(apprenticeshipWithdrawalDate);

        if (CompletedBeforeEndOfQualifyingPeriod(englishAndMaths, endDates.CompletionDate))
            return [];

        // If the course dates don't span a census date (i.e. course only exists in one month and ends before the census date), we still want to pay for that course in a single instalment for that month
        if (IsSingleMonthCourse(englishAndMaths))
            return [CreateInstalment(englishAndMaths.Key, englishAndMaths.EndDate, englishAndMaths.Amount)];

        var context = BuildCalculationContext(englishAndMaths, endDates);

        GenerateMonthlyInstalments(context);
        ApplyEarlyCompletionAdjustment(context);
        ApplyWithdrawalRules(context);
        ApplyLastDayOfCourseRule(context);

        return context.Instalments;
    }

    private static bool IsInvalidCourse(EnglishAndMaths mathsAndEnglish)
    {
        return mathsAndEnglish.StartDate > mathsAndEnglish.EndDate;
    }

    private static bool CompletedBeforeEndOfQualifyingPeriod(EnglishAndMaths englishAndMaths, DateTime? completionDate)
    {
        return completionDate.HasValue &&
               !LearnerQualifiesForEarnings(englishAndMaths.StartDate, englishAndMaths.EndDate, completionDate.Value);
    }

    private static bool IsSingleMonthCourse(EnglishAndMaths mathsAndEnglish)
    {
        return mathsAndEnglish.StartDate.Month == mathsAndEnglish.EndDate.Month && mathsAndEnglish.StartDate.Year == mathsAndEnglish.EndDate.Year;
    }

    private static InstalmentCalculationContext BuildCalculationContext(EnglishAndMaths mathsAndEnglish, EnglishAndMathsEndDates endDates)
    {
        var lastCensusDate = mathsAndEnglish.EndDate.LastCensusDate();
        var paymentDate = mathsAndEnglish.StartDate.LastDayOfMonth();

        var adjustedAmount =
            mathsAndEnglish.CombinedFundingAdjustmentPercentage is > 0
                ? mathsAndEnglish.Amount * mathsAndEnglish.CombinedFundingAdjustmentPercentage.Value
                : mathsAndEnglish.Amount;

        return new InstalmentCalculationContext(
            mathsAndEnglish,
            endDates,
            lastCensusDate,
            paymentDate,
            adjustedAmount
        );
    }

    private static void GenerateMonthlyInstalments(InstalmentCalculationContext context)
    {
        foreach (var periodInLearning in context.MathsAndEnglish.PeriodsInLearning)
        {
            var paymentDate = periodInLearning.StartDate.LastDayOfMonth();
            var lastCensusDate = periodInLearning.OriginalExpectedEndDate.LastCensusDate();
            
            var numberOfInstalments = CalculateNumberOfInstalments(paymentDate, lastCensusDate);
            if (numberOfInstalments <= 0)
                numberOfInstalments = 1;

            var monthlyAmount = context.AmountOutStanding / numberOfInstalments;

            while (paymentDate <= periodInLearning.EffectiveEndDate && paymentDate <= periodInLearning.OriginalExpectedEndDate)
            {
                context.Instalments.Add(CreateInstalment(context.MathsAndEnglish.Key, paymentDate, monthlyAmount));

                paymentDate = paymentDate.AddDays(1).AddMonths(1).AddDays(-1);
            }
        }
    }

    private static void ApplyEarlyCompletionAdjustment(InstalmentCalculationContext context)
    {
        var mathsAndEnglish = context.MathsAndEnglish;
        var completionDate = context.CompletionDate;

        if (!completionDate.HasValue ||
            completionDate >= mathsAndEnglish.EndDate)
            return;

        var completionCensusDate = completionDate.Value.LastDayOfMonth();
        var completionIsOnCensusDate = completionDate.Value.Date == completionCensusDate.Date;
        var completedInFirstMonth = completionCensusDate.Date == mathsAndEnglish.StartDate.LastDayOfMonth().Date;

        // A completion in the first month is paid as a single balancing instalment, even on the census date
        var paymentDateToAdjust = completionIsOnCensusDate && !completedInFirstMonth
            ? completionCensusDate.AddMonths(1).LastDayOfMonth()
            : completionCensusDate;

        while (paymentDateToAdjust <= mathsAndEnglish.EndDate.LastCensusDate())
        {
            context.Instalments.RemoveAll(x =>
                x.AcademicYear == paymentDateToAdjust.ToAcademicYear() &&
                x.DeliveryPeriod == paymentDateToAdjust.ToDeliveryPeriod());

            paymentDateToAdjust = paymentDateToAdjust.AddMonths(1).LastDayOfMonth();
        }

        if (context.AmountOutStanding != 0)
            context.Instalments.Add(
                CreateInstalment(
                    mathsAndEnglish.Key,
                    completionCensusDate,
                    context.AmountOutStanding,
                    EnglishAndMathsInstalmentType.Balancing));
    }

    private static void ApplyWithdrawalRules(InstalmentCalculationContext context)
    {
        var startDate = context.MathsAndEnglish.StartDate;
        var endDate = context.MathsAndEnglish.EndDate;
        var withdrawalDate = context.WithdrawalDate;

        // Remove all instalments if the withdrawal date is before the end of the qualifying period
        if (withdrawalDate.HasValue && 
            !LearnerQualifiesForEarnings(startDate, endDate, withdrawalDate.Value))
            context.Instalments.Clear();

        // Remove all instalments if the learner was not in learning on a census date before withdrawing
        if (withdrawalDate.HasValue &&
            withdrawalDate.Value < startDate.LastDayOfMonth())
            context.Instalments.Clear();
    }

    private static void ApplyLastDayOfCourseRule(InstalmentCalculationContext context)
    {
        if (context.ActualEndDate.HasValue)
        {
            DeleteAfterDate(context.Instalments, context.ActualEndDate.Value);
        }
    }

    internal static bool LearnerQualifiesForEarnings(DateTime startDate, DateTime endDate, DateTime lastDayInLearning)
    {
        var qualifyingPeriodDays = QualifyingPeriod.GetQualifyingPeriodDays(startDate, endDate);

        return lastDayInLearning >= startDate.AddDays(qualifyingPeriodDays - 1);
    }

    private static void DeleteAfterDate(
        List<EnglishAndMathsInstalmentEntity> instalments,
        DateTime cutoff)
    {
        var instalmentsToKeep = GetEnglishAndMathsEarningsToKeep(instalments, cutoff);

        instalments.RemoveAll(x => !instalmentsToKeep.Any(y => y.Key == x.Key));

    }

    private static List<EnglishAndMathsInstalmentEntity> GetEnglishAndMathsEarningsToKeep(List<EnglishAndMathsInstalmentEntity> instalments, DateTime? lastDayOfLearning)
    {
        if (!lastDayOfLearning.HasValue)
        {
            return instalments;
        }

        var academicYear = lastDayOfLearning.Value.ToAcademicYear();
        var deliveryPeriod = lastDayOfLearning.Value.ToDeliveryPeriod();

        return instalments
            .Where(x =>
                x.Type != EnglishAndMathsInstalmentType.Regular.ToString() //keep non-regular instalments
                ||
                (
                    x.AcademicYear < academicYear //keep earnings from previous academic years
                    || x.AcademicYear == academicYear && x.DeliveryPeriod < deliveryPeriod //keep earnings from previous delivery periods in the same academic year
                    || x.AcademicYear == academicYear && x.DeliveryPeriod == deliveryPeriod
                        && lastDayOfLearning.Value.Day == DateTime.DaysInMonth(lastDayOfLearning.Value.Year, lastDayOfLearning.Value.Month)) //keep earnings in the last delivery period of learning if the learner is in learning on the census date
            )
            .ToList();

    }

    private static EnglishAndMathsInstalmentEntity CreateInstalment(Guid key, DateTime dateTime, decimal amount, EnglishAndMathsInstalmentType instalmentType = EnglishAndMathsInstalmentType.Regular)
    {
        return new EnglishAndMathsInstalmentEntity(
             key,
             dateTime.ToAcademicYear(),
             dateTime.ToDeliveryPeriod(),
             amount,
             instalmentType.ToString());
    }

    private static int CalculateNumberOfInstalments(DateTime startDate, DateTime endDate)
    {
        return
            ((endDate.Year - startDate.Year) * 12 +
             endDate.Month - startDate.Month) + 1;
    }
}

internal class InstalmentCalculationContext
{
    internal List<EnglishAndMathsInstalmentEntity> Instalments { get; } = new List<EnglishAndMathsInstalmentEntity>();
    internal EnglishAndMaths MathsAndEnglish { get; private set; }
    internal DateTime? WithdrawalDate { get; private set; }
    internal DateTime? CompletionDate { get; private set; }
    internal DateTime? ActualEndDate => WithdrawalDate ?? CompletionDate ?? MathsAndEnglish.PauseDate;
    internal DateTime LastCensusDate { get; private set; }
    internal DateTime FirstPaymentDate { get; private set; }

    /// <summary> This is the course amount adjusted for prior learning </summary>
    private readonly decimal _adjustedCourseAmount;
    internal decimal AdjustedCourseAmount => _adjustedCourseAmount;

    internal decimal AmountOutStanding => _adjustedCourseAmount - Instalments.Sum(x => x.Amount);

    internal InstalmentCalculationContext(EnglishAndMaths mathsAndEnglish, EnglishAndMathsEndDates endDates, DateTime lastCensusDate, DateTime firstPaymentDate, decimal adjustedCourseTotal)
    {
        MathsAndEnglish = mathsAndEnglish;
        WithdrawalDate = endDates.WithdrawalDate;
        CompletionDate = endDates.CompletionDate;
        LastCensusDate = lastCensusDate;
        FirstPaymentDate = firstPaymentDate;
        _adjustedCourseAmount = adjustedCourseTotal;
    }

}