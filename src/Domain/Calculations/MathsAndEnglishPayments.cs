using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Apprenticeship;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Calculations;

public static class MathsAndEnglishPayments
{
    public static MathsAndEnglish GenerateMathsAndEnglishPayments(DateTime startDate, DateTime endDate, string course, decimal amount)
    {
        var instalments = new List<MathsAndEnglishInstalment>();

        if (startDate > endDate) return new MathsAndEnglish(startDate, endDate, course, amount, instalments);
        

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