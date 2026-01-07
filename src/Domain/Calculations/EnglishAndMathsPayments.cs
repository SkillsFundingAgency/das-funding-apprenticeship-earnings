using SFA.DAS.Funding.ApprenticeshipEarnings.DataAccess.Entities;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Apprenticeship;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Extensions;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Calculations;

public static class EnglishAndMathsPayments
{
    public static List<MathsAndEnglishInstalmentModel> GenerateInstalments(MathsAndEnglish mathsAndEnglish)
    {
        var instalments = new List<MathsAndEnglishInstalmentModel>();

        // This is invalid, it should never happen but should not result in any payments
        if (mathsAndEnglish.StartDate > mathsAndEnglish.EndDate) return instalments;

        // If the course dates don't span a census date (i.e. course only exists in one month and ends before the census date), we still want to pay for that course in a single instalment for that month
        if (mathsAndEnglish.StartDate.Month == mathsAndEnglish.EndDate.Month && mathsAndEnglish.StartDate.Year == mathsAndEnglish.EndDate.Year)
            return [ CreateInstalment(mathsAndEnglish.Key, mathsAndEnglish.EndDate,mathsAndEnglish.Amount) ];

        var lastCensusDate = mathsAndEnglish.EndDate.LastCensusDate();
        var paymentDate = mathsAndEnglish.StartDate.LastDayOfMonth();

        // Adjust for prior learning if applicable
        var adjustedAmount = mathsAndEnglish.PriorLearningAdjustmentPercentage.HasValue && mathsAndEnglish.PriorLearningAdjustmentPercentage != 0
            ? mathsAndEnglish.Amount * mathsAndEnglish.PriorLearningAdjustmentPercentage.Value / 100m
            : mathsAndEnglish.Amount;

        var numberOfInstalments = ((lastCensusDate.Year - paymentDate.Year) * 12 + lastCensusDate.Month - paymentDate.Month) + 1;
        var monthlyAmount = adjustedAmount / numberOfInstalments;

        while (paymentDate <= lastCensusDate)
        {
            instalments.Add(CreateInstalment(mathsAndEnglish.Key, paymentDate, monthlyAmount));

            paymentDate = paymentDate.AddDays(1).AddMonths(1).AddDays(-1);
        }

        // If an actual end date has been set and is before the planned end date then the learner has completed early and adjustments need to be made
        if (mathsAndEnglish.ActualEndDate.HasValue && mathsAndEnglish.ActualEndDate < mathsAndEnglish.EndDate)
        {
            var paymentDateToAdjust = mathsAndEnglish.ActualEndDate.Value.LastDayOfMonth();
            var balancingCount = 0;

            while (paymentDateToAdjust <= mathsAndEnglish.EndDate.LastCensusDate())
            {
                instalments.RemoveAll(x =>
                    x.AcademicYear == paymentDateToAdjust.ToAcademicYear() &&
                    x.DeliveryPeriod == paymentDateToAdjust.ToDeliveryPeriod());  //todo soft delete these too?

                paymentDateToAdjust = paymentDateToAdjust.AddMonths(1).LastDayOfMonth();
                balancingCount++;
            }

            var balancingAmount = balancingCount * monthlyAmount;

            instalments.Add(
                CreateInstalment(
                    mathsAndEnglish.Key, 
                    mathsAndEnglish.ActualEndDate.Value.LastDayOfMonth(), 
                    balancingAmount,
                    MathsAndEnglishInstalmentType.Balancing));
        }

        // Remove all instalments if the withdrawal date is before the end of the qualifying period
        if (mathsAndEnglish.WithdrawalDate.HasValue && !WithdrawnLearnerQualifiesForEarnings(mathsAndEnglish.StartDate, mathsAndEnglish.EndDate, mathsAndEnglish.WithdrawalDate.Value))
            instalments.Clear();

        // Special case if the withdrawal date is on/after the start date but before a census date we should make one instalment for the first month of learning
        if (mathsAndEnglish.WithdrawalDate.HasValue && mathsAndEnglish.WithdrawalDate.Value >= mathsAndEnglish.StartDate && mathsAndEnglish.WithdrawalDate.Value < mathsAndEnglish.StartDate.LastDayOfMonth())
            instalments.Add(CreateInstalment(mathsAndEnglish.Key, mathsAndEnglish.StartDate, monthlyAmount));

        if (mathsAndEnglish.LastDayOfCourse.HasValue)
        {
            DeleteAfterDate(instalments, mathsAndEnglish.LastDayOfCourse.Value, mathsAndEnglish.StartDate);
        }

        return instalments;
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
}