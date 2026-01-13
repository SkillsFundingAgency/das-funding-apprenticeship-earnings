using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Apprenticeship;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Extensions;

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

}
