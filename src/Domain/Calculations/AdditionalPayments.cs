using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Extensions;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Models.Apprenticeship;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Calculations;

internal static class AdditionalPayments
{
    internal static List<AdditionalPayment> RemoveAfterLastDayOfLearning(List<AdditionalPayment> additionalPayments, DateTime lastDayOfLearning)
    {
        var academicYear = lastDayOfLearning.ToAcademicYear();
        var deliveryPeriod = lastDayOfLearning.ToDeliveryPeriod();
        var isCensusDay = lastDayOfLearning.Day == DateTime.DaysInMonth(lastDayOfLearning.Year, lastDayOfLearning.Month);

        var additionalPaymentsToKeep = additionalPayments
            .Where(x =>
                    x.AdditionalPaymentType == InstalmentTypes.LearningSupport || // always keep LearningSupport
                    x.AcademicYear < academicYear || // keep earnings from previous academic years
                    (x.AcademicYear == academicYear && x.DeliveryPeriod < deliveryPeriod) || // keep earlier periods in same year
                    (x.AcademicYear == academicYear && x.DeliveryPeriod == deliveryPeriod && isCensusDay) || // keep current period if on census day
                    (x.DueDate <= lastDayOfLearning && x.IsIncentivePayment()) // keep incentive payments due on or before last day of learning
            )
            .ToList();

        additionalPayments.RemoveAll(ap => !additionalPaymentsToKeep.Contains(ap));

        return additionalPayments;
    }

    /// <summary>
    /// Learning support is only earned for months covered by a period in learning that qualifies for earnings.
    /// A payment is removed when no qualifying period in learning spans its delivery period (this also removes any after the last day of learning).
    /// </summary>
    internal static List<AdditionalPayment> RemoveLearningSupportNotQualifying(List<AdditionalPayment> additionalPayments, IReadOnlyCollection<ApprenticeshipPeriodInLearning> periodsInLearning, DateTime? lastDayOfLearning)
    {
        if (!lastDayOfLearning.HasValue || periodsInLearning.Count == 0)
        {
            return additionalPayments;
        }

        additionalPayments.RemoveAll(x =>
            x.AdditionalPaymentType == InstalmentTypes.LearningSupport &&
            !periodsInLearning.Any(p => p.QualifiesForEarnings(lastDayOfLearning.Value) && p.SpansDeliveryPeriod(x.AcademicYear, x.DeliveryPeriod, lastDayOfLearning.Value)));

        return additionalPayments;
    }

}
