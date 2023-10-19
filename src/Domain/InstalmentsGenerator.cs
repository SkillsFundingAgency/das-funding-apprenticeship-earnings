using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.ApprenticeshipFunding;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.Domain;

public interface IInstalmentsGenerator
{
    public List<Earning> Generate(decimal total, DateTime startDate, DateTime endDate);
    public List<Earning> Generate(decimal total, DateTime startDate, DateTime endDate, List<Earning> existingEarnings);
}

public class InstalmentsGenerator : IInstalmentsGenerator
{
    public List<Earning> Generate(decimal total, DateTime startDate, DateTime endDate)
    {
        var installments = new List<Earning>();

        var startDateMonth = new DateTime(startDate.Year, startDate.Month, 1);
        var endDateMonth = GetLastPaymentMonth(endDate);

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
            installment.Amount = decimal.Round(installmentAmount, 5);
        }

        return installments;
    }

    public List<Earning> Generate(decimal total, DateTime priceDateChange, DateTime endDate, List<Earning> existingEarnings)
    {
        var installments = new List<Earning>();

        var lastPaidMonth = GetLastPaymentMonth(priceDateChange);
        installments = existingEarnings.Where(x => x.AcademicYear.ToDateTime(x.DeliveryPeriod) <= lastPaidMonth).ToList();

        var totalExistingEarnings = installments.Sum(x => x.Amount);
        var amountLeftToPay = total - totalExistingEarnings;
        var nextPayMonth = lastPaidMonth.AddMonths(1);

        var newInstallments = Generate(amountLeftToPay, nextPayMonth, endDate);

        installments.AddRange(newInstallments);
        return installments;
    }

    private DateTime GetLastPaymentMonth(DateTime endDate)
    {
        if (endDate.Day == DateTime.DaysInMonth(endDate.Year, endDate.Month))
            return new DateTime(endDate.Year, endDate.Month, 1);

        return new DateTime(endDate.Year, endDate.Month, 1).AddMonths(-1);
    }
}