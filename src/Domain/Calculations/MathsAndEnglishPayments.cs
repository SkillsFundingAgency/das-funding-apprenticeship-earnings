using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Apprenticeship;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Extensions;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Calculations;

public class GenerateMathsAndEnglishPaymentsCommand
{
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public string Course { get; set; }
    public decimal Amount { get; set; }
    public DateTime? ActualEndDate { get; set; }

    public GenerateMathsAndEnglishPaymentsCommand(DateTime startDate, DateTime endDate, string course, decimal amount, DateTime? actualEndDate = null)
    {
        StartDate = startDate;
        EndDate = endDate;
        Course = course;
        Amount = amount;
        ActualEndDate = actualEndDate;
    }
}

public static class MathsAndEnglishPayments
{
    public static MathsAndEnglish GenerateMathsAndEnglishPayments(GenerateMathsAndEnglishPaymentsCommand command)
    {
        var instalments = new List<MathsAndEnglishInstalment>();

        // This is invalid, it should never happen but should not result in any payments
        if (command.StartDate > command.EndDate) return new MathsAndEnglish(command.StartDate, command.EndDate, command.Course, command.Amount, instalments);
        
        // If the course dates don't span a census date (i.e. course only exists in one month and ends before the census date), we still want to pay for that course in a single instalment for that month
        if(command.StartDate.Month == command.EndDate.Month && command.StartDate.Year == command.EndDate.Year)
            return new MathsAndEnglish(command.StartDate, command.EndDate, command.Course, command.Amount, new List<MathsAndEnglishInstalment> { new(command.EndDate.ToAcademicYear(), command.EndDate.ToDeliveryPeriod(), command.Amount) });

        var lastCensusDate = command.EndDate.LastCensusDate();
        var paymentDate = command.StartDate.LastDayOfMonth();

        var numberOfInstalments = ((lastCensusDate.Year - paymentDate.Year) * 12 + lastCensusDate.Month - paymentDate.Month) + 1;
        var monthlyAmount = command.Amount / numberOfInstalments;

        while (paymentDate <= lastCensusDate)
        {
            instalments.Add(new MathsAndEnglishInstalment(
                paymentDate.ToAcademicYear(),
                paymentDate.ToDeliveryPeriod(),
                monthlyAmount
            ));

            paymentDate = paymentDate.AddDays(1).AddMonths(1).AddDays(-1);
        }

        // If an actual end date has been set and is before the planned end date then the learner has completed early and adjustments need to be made
        if (command.ActualEndDate.HasValue && command.ActualEndDate < command.EndDate)
        {
            var paymentDateToAdjust = command.ActualEndDate.Value.LastDayOfMonth();
            var balancingCount = 0;

            while (paymentDateToAdjust <= command.EndDate.LastCensusDate())
            {
                instalments.RemoveAll(x =>
                    x.AcademicYear == paymentDateToAdjust.ToAcademicYear() &&
                    x.DeliveryPeriod == paymentDateToAdjust.ToDeliveryPeriod());

                paymentDateToAdjust = paymentDateToAdjust.AddDays(1).AddMonths(1).AddDays(-1);
                balancingCount++;
            }

            var balancingAmount = balancingCount * monthlyAmount;

            instalments.Add(new MathsAndEnglishInstalment(command.ActualEndDate.Value.LastDayOfMonth().ToAcademicYear(), command.ActualEndDate.Value.LastDayOfMonth().ToDeliveryPeriod(), balancingAmount));
        }

        return new MathsAndEnglish(command.StartDate, command.EndDate, command.Course, command.Amount, instalments);
    }
}