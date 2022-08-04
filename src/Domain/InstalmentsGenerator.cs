using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.ApprenticeshipFunding;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.Domain;

public interface IInstalmentsGenerator
{
    public List<Earning> Generate(decimal total, DateTime startDate, DateTime endDate);
}

public class InstalmentsGenerator : IInstalmentsGenerator
{
    public List<Earning> Generate(decimal total, DateTime startDate, DateTime endDate)
    {
        var installments = new List<Earning>();

        var startDateMonth = new DateTime(startDate.Year, startDate.Month, 1);
        var endDateMonth = new DateTime(endDate.Year, endDate.Month, 1);

        while (startDateMonth <= endDateMonth)
        {
            installments.Add(new Earning
            {
                DeliveryPeriod = startDateMonth.ToDeliveryPeriod(),
                AcademicYear = startDateMonth.ToAcademicYear()
            });
            startDateMonth = startDateMonth.AddMonths(1);
        }

        var installmentAmount = total / installments.Count;

        foreach (var installment in installments)
        {
            installment.Amount = installmentAmount;
        }

        return installments;
    }
}