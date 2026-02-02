using SFA.DAS.Funding.ApprenticeshipEarnings.DataAccess.Entities;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Apprenticeship;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Extensions;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Calculations;

public static class EnglishAndMathsPayments
{
    public static List<MathsAndEnglishInstalmentModel> GenerateInstalments(MathsAndEnglish mathsAndEnglish)
    {
        if (IsInvalidCourse(mathsAndEnglish))
            return [];

        // If the course dates don't span a census date (i.e. course only exists in one month and ends before the census date), we still want to pay for that course in a single instalment for that month
        if (IsSingleMonthCourse(mathsAndEnglish))
            return [CreateInstalment(mathsAndEnglish.Key, mathsAndEnglish.EndDate, mathsAndEnglish.Amount)];

        var context = BuildCalculationContext(mathsAndEnglish);

        GenerateMonthlyInstalments(context);
        ApplyEarlyCompletionAdjustment(context);
        ApplyWithdrawalRules(context);
        ApplyLastDayOfCourseRule(context);

        return context.Instalments;
    }

    private static bool IsInvalidCourse(MathsAndEnglish mathsAndEnglish)
    {
        return mathsAndEnglish.StartDate > mathsAndEnglish.EndDate;
    }

    private static bool IsSingleMonthCourse(MathsAndEnglish mathsAndEnglish)
    {
        return mathsAndEnglish.StartDate.Month == mathsAndEnglish.EndDate.Month && mathsAndEnglish.StartDate.Year == mathsAndEnglish.EndDate.Year;
    }

    private static InstalmentCalculationContext BuildCalculationContext(MathsAndEnglish mathsAndEnglish)
    {
        var lastCensusDate = mathsAndEnglish.EndDate.LastCensusDate();
        var paymentDate = mathsAndEnglish.StartDate.LastDayOfMonth();

        var adjustedAmount =
            mathsAndEnglish.PriorLearningAdjustmentPercentage is > 0
                ? mathsAndEnglish.Amount * mathsAndEnglish.PriorLearningAdjustmentPercentage.Value / 100m
                : mathsAndEnglish.Amount;

        return new InstalmentCalculationContext(
            mathsAndEnglish,
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

            var monthlyAmount = context.AmountOutStanding / numberOfInstalments;

            while (paymentDate <= periodInLearning.EndDate)
            {
                context.Instalments.Add(CreateInstalment(context.MathsAndEnglish.Key, paymentDate, monthlyAmount));

                paymentDate = paymentDate.AddDays(1).AddMonths(1).AddDays(-1);
            }
        }

    }

    private static void ApplyEarlyCompletionAdjustment(InstalmentCalculationContext context)
    {
        var mathsAndEnglish = context.MathsAndEnglish;

        if (!mathsAndEnglish.CompletionDate.HasValue ||
            mathsAndEnglish.CompletionDate >= mathsAndEnglish.EndDate)
            return;

        var paymentDateToAdjust = mathsAndEnglish.CompletionDate.Value.LastDayOfMonth();

        while (paymentDateToAdjust <= mathsAndEnglish.EndDate.LastCensusDate())
        {
            context.Instalments.RemoveAll(x =>
                x.AcademicYear == paymentDateToAdjust.ToAcademicYear() &&
                x.DeliveryPeriod == paymentDateToAdjust.ToDeliveryPeriod());

            paymentDateToAdjust = paymentDateToAdjust.AddMonths(1).LastDayOfMonth();
        }

        context.Instalments.Add(
            CreateInstalment(
                mathsAndEnglish.Key,
                mathsAndEnglish.CompletionDate.Value.LastDayOfMonth(),
                context.AmountOutStanding,
                MathsAndEnglishInstalmentType.Balancing));
    }

    private static void ApplyWithdrawalRules(InstalmentCalculationContext context)
    {
        var startDate = context.MathsAndEnglish.StartDate;
        var endDate = context.MathsAndEnglish.EndDate;
        var withdrawalDate = context.MathsAndEnglish.WithdrawalDate;

        // Remove all instalments if the withdrawal date is before the end of the qualifying period
        if (withdrawalDate.HasValue && 
            !WithdrawnLearnerQualifiesForEarnings(startDate, endDate, withdrawalDate.Value))
            context.Instalments.Clear();

        // Special case if the withdrawal date is on/after the start date but before a census date we should make one instalment for the first month of learning
        if (withdrawalDate.HasValue && 
            withdrawalDate.Value >= startDate &&
            withdrawalDate.Value < startDate.LastDayOfMonth())
        {
            var numberOfInstalments = CalculateNumberOfInstalments(context.FirstPaymentDate, context.LastCensusDate);
            var monthlyAmount = context.AdjustedCourseAmount / numberOfInstalments;
            context.Instalments.Add(CreateInstalment(context.MathsAndEnglish.Key, startDate, monthlyAmount));
        }   
    }

    private static void ApplyLastDayOfCourseRule(InstalmentCalculationContext context)
    {
        if (context.MathsAndEnglish.ActualEndDate.HasValue)
        {
            DeleteAfterDate(context.Instalments, context.MathsAndEnglish.ActualEndDate.Value, context.MathsAndEnglish.StartDate);
        }
    }

    private static bool WithdrawnLearnerQualifiesForEarnings(DateTime startDate, DateTime endDate, DateTime withdrawalDate)
    {
        var plannedLength = (endDate - startDate).TotalDays + 1;
        var actualLength = (withdrawalDate - startDate).TotalDays + 1;

        if (plannedLength >= 168)
            return actualLength >= 42;
        if (plannedLength >= 14)
            return actualLength >= 14;

        return actualLength >= 1;
    }

    private static void DeleteAfterDate(
        List<MathsAndEnglishInstalmentModel> instalments,
        DateTime cutoff,
        DateTime startDate)
    {
        var instalmentsToKeep = GetEnglishAndMathsEarningsToKeep(instalments, startDate, cutoff);

        instalments.RemoveAll(x => !instalmentsToKeep.Any(y => y.Key == x.Key));

    }

    private static List<MathsAndEnglishInstalmentModel> GetEnglishAndMathsEarningsToKeep(List<MathsAndEnglishInstalmentModel> instalments, DateTime startDate, DateTime? lastDayOfLearning)
    {
        if (!lastDayOfLearning.HasValue)
        {
            return instalments;
        }

        var academicYear = lastDayOfLearning.Value.ToAcademicYear();
        var deliveryPeriod = lastDayOfLearning.Value.ToDeliveryPeriod();
        var startAcademicYear = startDate.ToAcademicYear();
        var startDeliveryPeriod = startDate.ToDeliveryPeriod();

        return instalments
            .Where(x =>
                x.Type != MathsAndEnglishInstalmentType.Regular.ToString() //keep non-regular instalments
                ||
                (
                    x.AcademicYear < academicYear //keep earnings from previous academic years
                    || x.AcademicYear == academicYear && x.DeliveryPeriod < deliveryPeriod //keep earnings from previous delivery periods in the same academic year
                    || x.AcademicYear == academicYear && x.DeliveryPeriod == deliveryPeriod
                        && lastDayOfLearning.Value.Day == DateTime.DaysInMonth(lastDayOfLearning.Value.Year, lastDayOfLearning.Value.Month) //keep earnings in the last delivery period of learning if the learner is in learning on the census date
                    || x.AcademicYear == academicYear && x.DeliveryPeriod == deliveryPeriod
                        && startAcademicYear == academicYear && startDeliveryPeriod == deliveryPeriod
                        && lastDayOfLearning.Value > startDate) // special case if the withdrawal date is on/after the start date but before a census date we should keep the instalment for the first month of learning
            )
            .ToList();

    }

    private static MathsAndEnglishInstalmentModel CreateInstalment(Guid key, DateTime dateTime, decimal amount, MathsAndEnglishInstalmentType instalmentType = MathsAndEnglishInstalmentType.Regular)
    {
        return new MathsAndEnglishInstalmentModel(
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
    internal List<MathsAndEnglishInstalmentModel> Instalments { get; } = new List<MathsAndEnglishInstalmentModel>();
    internal MathsAndEnglish MathsAndEnglish { get; private set; }
    internal DateTime LastCensusDate { get; private set; }
    internal DateTime FirstPaymentDate { get; private set; }

    /// <summary> This is the course amount adjusted for prior learning </summary>
    private readonly decimal _adjustedCourseAmount;
    internal decimal AdjustedCourseAmount => _adjustedCourseAmount;

    internal decimal AmountOutStanding => _adjustedCourseAmount - Instalments.Sum(x => x.Amount);

    internal InstalmentCalculationContext(MathsAndEnglish mathsAndEnglish, DateTime lastCensusDate, DateTime firstPaymentDate, decimal adjustedCourseTotal)
    {
        MathsAndEnglish = mathsAndEnglish;
        LastCensusDate = lastCensusDate;
        FirstPaymentDate = firstPaymentDate;
        _adjustedCourseAmount = adjustedCourseTotal;
    }

}