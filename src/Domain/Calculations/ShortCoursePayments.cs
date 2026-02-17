using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Apprenticeship;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.ApprenticeshipFunding;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Extensions;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Calculations;

public static class ShortCoursePayments
{
    public static List<Instalment> GenerateShortCoursePayments(decimal totalPrice, DateTime startDate, DateTime endDate, Guid priceKey)
    {
        var payments = new List<OnProgramPayment>();

        var duration = (endDate - startDate).Days + 1;
        var firstPaymentDate = startDate.AddDays(Math.Floor(duration * 0.3) - 1);

        payments.Add(new OnProgramPayment
        {
            AcademicYear = firstPaymentDate.ToAcademicYear(),
            DeliveryPeriod = firstPaymentDate.ToDeliveryPeriod(),
            Amount = totalPrice * 0.3m,
            PriceKey = priceKey
        });

        payments.Add(new OnProgramPayment
        {
            AcademicYear = endDate.ToAcademicYear(),
            DeliveryPeriod = endDate.ToDeliveryPeriod(),
            Amount = totalPrice * 0.7m,
            PriceKey = priceKey
        });

        return payments.Select(x => new Instalment(x.AcademicYear, x.DeliveryPeriod, x.Amount, x.PriceKey)).ToList();
    }
}