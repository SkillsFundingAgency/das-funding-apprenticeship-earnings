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
        var payments = new List<OnProgramPayment>();

        var duration = (endDate - startDate).Days + 1;
        var firstPaymentDate = startDate.AddDays(Math.Floor(duration * FirstPaymentDurationPercentage) - 1);

        payments.Add(new OnProgramPayment
        {
            AcademicYear = firstPaymentDate.ToAcademicYear(),
            DeliveryPeriod = firstPaymentDate.ToDeliveryPeriod(),
            Amount = totalPrice * FirstPaymentPortionPercentage,
            PriceKey = priceKey
        });

        payments.Add(new OnProgramPayment
        {
            AcademicYear = completionDate?.ToAcademicYear() ?? endDate.ToAcademicYear(),
            DeliveryPeriod = completionDate?.ToDeliveryPeriod() ?? endDate.ToDeliveryPeriod(),
            Amount = totalPrice * SecondPaymentPortionPercentage,
            PriceKey = priceKey
        });

        return payments.Select(x => new Instalment(x.AcademicYear, x.DeliveryPeriod, x.Amount, x.PriceKey)).ToList();
    }
}