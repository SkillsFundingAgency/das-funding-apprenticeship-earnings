using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Extensions;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Models.Apprenticeship;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Calculations;

public static class LearningSupportPayments
{
    /// <summary>
    /// Calculate: generates the learning support payments, each with the standard learning support amount.
    /// </summary>
    public static List<AdditionalPayment> GenerateLearningSupportPayments(DateTime startDate, DateTime endDate)
    {
        return GenerateSchedule(startDate, endDate, AdditionalPaymentAmounts.LearningSupport);
    }

    /// <summary>
    /// Update (store only): generates the same schedule as <see cref="GenerateLearningSupportPayments"/> so the learning support
    /// periods are recorded, but with a zero amount so that no earnings are held. The rows have identical keys
    /// (academic year, delivery period, due date, type), so calculating later updates the amounts in place.
    /// </summary>
    public static List<AdditionalPayment> GenerateLearningSupportPaymentsWithoutEarnings(DateTime startDate, DateTime endDate)
    {
        return GenerateSchedule(startDate, endDate, 0m);
    }

    private static List<AdditionalPayment> GenerateSchedule(DateTime startDate, DateTime endDate, decimal amount)
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
                amount,
                paymentDate,
                InstalmentTypes.LearningSupport
            ));

            paymentDate = paymentDate.AddDays(1).AddMonths(1).AddDays(-1);
        }
        return learningSupportPayments;
    }
}