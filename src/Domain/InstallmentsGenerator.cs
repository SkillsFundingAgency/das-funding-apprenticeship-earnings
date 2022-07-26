namespace SFA.DAS.Funding.ApprenticeshipEarnings.Domain;

public interface IInstallmentsGenerator
{
    public List<EarningsInstallment> Generate(decimal total, DateTime startDate, DateTime endDate);
}

public class InstallmentsGenerator : IInstallmentsGenerator
{
    public List<EarningsInstallment> Generate(decimal total, DateTime startDate, DateTime endDate)
    {
        var installments = new List<EarningsInstallment>();

        var startDateMonth = new DateTime(startDate.Year, startDate.Month, 1);
        var endDateMonth = new DateTime(endDate.Year, endDate.Month, 1);

        while (startDateMonth <= endDateMonth)
        {
            installments.Add(new EarningsInstallment
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