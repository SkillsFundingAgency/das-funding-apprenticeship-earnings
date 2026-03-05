using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Extensions;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Models;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Models.ShortCourse;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Calculations;

public static class ShortCoursePayments
{
    private const double FirstPaymentDurationPercentage = 0.3;
    private const decimal FirstPaymentPortionPercentage = 0.3m;
    private const decimal SecondPaymentPortionPercentage = 0.7m;

    public static List<ShortCourseInstalment> GenerateShortCoursePayments(decimal totalPrice, DateTime startDate, DateTime endDate, DateTime? completionDate)
    {
        var payments = new List<ShortCourseInstalment>();

        var duration = (endDate - startDate).Days + 1;
        var firstPaymentDate = startDate.AddDays(Math.Floor(duration * FirstPaymentDurationPercentage) - 1);

        payments.Add(new ShortCourseInstalment
        (
            firstPaymentDate.ToAcademicYear(),
            firstPaymentDate.ToDeliveryPeriod(),
            totalPrice * FirstPaymentPortionPercentage,
            InstalmentType.Regular
        ));

        payments.Add(new ShortCourseInstalment
        (
            completionDate?.ToAcademicYear() ?? endDate.ToAcademicYear(),
            completionDate?.ToDeliveryPeriod() ?? endDate.ToDeliveryPeriod(),
            totalPrice * SecondPaymentPortionPercentage,
            InstalmentType.Completion
        ));

        return payments;
    }
}