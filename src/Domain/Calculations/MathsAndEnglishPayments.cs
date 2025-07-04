﻿using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Apprenticeship;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Extensions;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Calculations;

public static class MathsAndEnglishPayments
{
    public static MathsAndEnglish GenerateMathsAndEnglishPayments(DateTime startDate, DateTime endDate, string course, decimal amount)
    {
        var instalments = new List<MathsAndEnglishInstalment>();

        // This is invalid, it should never happen but should not result in any payments
        if (startDate > endDate) return new MathsAndEnglish(startDate, endDate, course, amount, instalments);
        
        // If the course dates don't span a census date (i.e. course only exists in one month and ends before the census date), we still want to pay for that course in a single instalment for that month
        if(startDate.Month == endDate.Month && startDate.Year == endDate.Year)
            return new MathsAndEnglish(startDate, endDate, course, amount, new List<MathsAndEnglishInstalment> { new(endDate.ToAcademicYear(), endDate.ToDeliveryPeriod(), amount) });

        var lastCensusDate = endDate.LastCensusDate();
        var paymentDate = startDate.LastDayOfMonth();

        var numberOfInstalments = ((lastCensusDate.Year - paymentDate.Year) * 12 + lastCensusDate.Month - paymentDate.Month) + 1;
        var monthlyAmount = amount / numberOfInstalments;

        while (paymentDate <= lastCensusDate)
        {
            instalments.Add(new MathsAndEnglishInstalment(
                paymentDate.ToAcademicYear(),
                paymentDate.ToDeliveryPeriod(),
                monthlyAmount
            ));

            paymentDate = paymentDate.AddDays(1).AddMonths(1).AddDays(-1);
        }

        return new MathsAndEnglish(startDate, endDate, course, amount, instalments);
    }
}