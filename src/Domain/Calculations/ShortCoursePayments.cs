using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Apprenticeship;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.ApprenticeshipFunding;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Extensions;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Calculations;

public static class ShortCoursePayments
{
    private const double FirstPaymentDurationPercentage = 0.3;
    private const decimal FirstPaymentPortionPercentage = 0.3m;
    private const decimal SecondPaymentPortionPercentage = 0.7m;

    public static List<Instalment> GenerateShortCoursePayments(decimal totalPrice, DateTime startDate, DateTime endDate, Guid priceKey, DateTime? completionDate)
    {
        var payments = new List<Instalment>();

        var duration = (endDate - startDate).Days + 1;
        var firstPaymentDate = startDate.AddDays(Math.Floor(duration * FirstPaymentDurationPercentage) - 1);

        payments.Add(new Instalment
        (
            firstPaymentDate.ToAcademicYear(),
            firstPaymentDate.ToDeliveryPeriod(),
            totalPrice * FirstPaymentPortionPercentage,
            priceKey,
            InstalmentType.Regular
        ));

        payments.Add(new Instalment
        (
            completionDate?.ToAcademicYear() ?? endDate.ToAcademicYear(),
            completionDate?.ToDeliveryPeriod() ?? endDate.ToDeliveryPeriod(),
            totalPrice * SecondPaymentPortionPercentage,
            priceKey,
            InstalmentType.Completion
        ));

        return payments;
    }
}