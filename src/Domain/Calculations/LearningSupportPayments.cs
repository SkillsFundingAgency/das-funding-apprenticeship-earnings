using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Apprenticeship;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;

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
        var paymentDate = startDate;

        while (paymentDate <= lastCensusDate)
        {
            learningSupportPayments.Add(new AdditionalPayment(
                paymentDate.ToAcademicYear(),
                paymentDate.ToDeliveryPeriod(),
                AdditionalPaymentAmounts.LearningSupport,
                paymentDate,
                InstalmentTypes.LearningSupport
            ));

            paymentDate = paymentDate.StartOfNextMonth();
        }
        return learningSupportPayments;
    }

    private static DateTime StartOfNextMonth(this DateTime date)
    {
        return new DateTime(date.Year, date.Month, 1).AddMonths(1);
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
