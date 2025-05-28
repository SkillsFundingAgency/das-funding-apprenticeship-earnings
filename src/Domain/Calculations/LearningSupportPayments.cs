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
        var paymentDate = startDate.LastDayOfMonth();

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
}