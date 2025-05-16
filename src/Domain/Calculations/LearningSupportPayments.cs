using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Apprenticeship;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Calculations;

public static class LearningSupportPayments
{
    public static List<AdditionalPayment> GenerateLearningSupportPayments(DateTime startDate, DateTime endDate)
    {
        var learningSupportPayments = new List<AdditionalPayment>();

        if (startDate > endDate)
        {
            return learningSupportPayments;
        }

        var lastCensusDate = endDate.LastCensusDate();
        var paymentDate = LastDayOfMonth(startDate);

        while (paymentDate <= lastCensusDate)
        {
            learningSupportPayments.Add(new AdditionalPayment(
                paymentDate.ToAcademicYear(),
                paymentDate.ToDeliveryPeriod(),
                AdditionalPaymentAmounts.LearningSupport,
                paymentDate,
                InstalmentTypes.LearningSupport
            ));

            paymentDate = paymentDate.AddDays(1).AddMonths(1).AddDays(-1);
        }
        return learningSupportPayments;
    }

    private static DateTime LastDayOfMonth(this DateTime date)
    {
        var day = DateTime.DaysInMonth(date.Year, date.Month);
        return new DateTime(date.Year, date.Month, day);
    }


    private static DateTime LastCensusDate(this DateTime date)
    {
        var nextMonth = date.AddMonths(1);
        var censusDateForMonth = new DateTime(nextMonth.Year, nextMonth.Month, 1).AddDays(-1);
        if(censusDateForMonth == date)
        {
            return date;
        }

        return new DateTime(date.Year, date.Month, 1).AddDays(-1);
    }
}
