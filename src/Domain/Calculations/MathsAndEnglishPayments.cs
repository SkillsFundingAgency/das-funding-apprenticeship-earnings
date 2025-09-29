using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Extensions;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Apprenticeship;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Calculations;


public class GenerateMathsAndEnglishPaymentsCommand
{
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public string Course { get; set; }
    public decimal Amount { get; set; }
    public DateTime? WithdrawalDate { get; set; }
    public DateTime? ActualEndDate { get; set; }
    public int? PriorLearningAdjustmentPercentage { get; set; }

    public GenerateMathsAndEnglishPaymentsCommand(DateTime startDate, DateTime endDate, string course, decimal amount,  DateTime? withdrawalDate = null, DateTime? actualEndDate = null, int? priorLearningAdjustmentPercentage = null)
    {
        StartDate = startDate;
        EndDate = endDate;
        Course = course;
        Amount = amount;
        WithdrawalDate = withdrawalDate;
        ActualEndDate = actualEndDate;
        PriorLearningAdjustmentPercentage = priorLearningAdjustmentPercentage;
    }
}

public static class MathsAndEnglishPayments
{
    public static MathsAndEnglish GenerateMathsAndEnglishPayments(GenerateMathsAndEnglishPaymentsCommand command)
    {
        var instalments = new List<MathsAndEnglishInstalment>();

        // This is invalid, it should never happen but should not result in any payments
        if (command.StartDate > command.EndDate) return new MathsAndEnglish(command.StartDate, command.EndDate, command.Course, command.Amount, instalments, command.WithdrawalDate, command.ActualEndDate, command.PriorLearningAdjustmentPercentage);

        // If the course dates don't span a census date (i.e. course only exists in one month and ends before the census date), we still want to pay for that course in a single instalment for that month
        if (command.StartDate.Month == command.EndDate.Month && command.StartDate.Year == command.EndDate.Year)
            return new MathsAndEnglish(command.StartDate,
                command.EndDate,
                command.Course,
                command.Amount,
                [
                    new(command.EndDate.ToAcademicYear(),
                        command.EndDate.ToDeliveryPeriod(),
                        command.Amount,
                        MathsAndEnglishInstalmentType.Regular)
                ],
                command.WithdrawalDate,
                command.ActualEndDate,
                command.PriorLearningAdjustmentPercentage);

        var lastCensusDate = command.EndDate.LastCensusDate();
        var paymentDate = command.StartDate.LastDayOfMonth();

        // Adjust for prior learning if applicable
        var adjustedAmount = command.PriorLearningAdjustmentPercentage.HasValue && command.PriorLearningAdjustmentPercentage != 0
            ? command.Amount * command.PriorLearningAdjustmentPercentage.Value / 100m
            : command.Amount;

        var numberOfInstalments = ((lastCensusDate.Year - paymentDate.Year) * 12 + lastCensusDate.Month - paymentDate.Month) + 1;
        var monthlyAmount = adjustedAmount / numberOfInstalments;

        while (paymentDate <= lastCensusDate)
        {
            instalments.Add(new MathsAndEnglishInstalment(
                paymentDate.ToAcademicYear(),
                paymentDate.ToDeliveryPeriod(),
                monthlyAmount,
                MathsAndEnglishInstalmentType.Regular
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

                paymentDateToAdjust = paymentDateToAdjust.AddMonths(1).LastDayOfMonth();
                balancingCount++;
            }

            var balancingAmount = balancingCount * monthlyAmount;

            instalments.Add(new MathsAndEnglishInstalment(command.ActualEndDate.Value.LastDayOfMonth().ToAcademicYear(),
                command.ActualEndDate.Value.LastDayOfMonth().ToDeliveryPeriod(),
                balancingAmount,
                MathsAndEnglishInstalmentType.Balancing));
        }

        // Remove instalments after the withdrawal date
        if (command.WithdrawalDate.HasValue)
            instalments.RemoveAll(x => x.DeliveryPeriod.GetCensusDate(x.AcademicYear) > command.WithdrawalDate.Value);

        // Special case if the withdrawal date is on/after the start date but before a census date we should make one instalment for the first month of learning
        if (command.WithdrawalDate.HasValue && command.WithdrawalDate.Value >= command.StartDate && command.WithdrawalDate.Value < command.StartDate.LastDayOfMonth())
            instalments.Add(new MathsAndEnglishInstalment(command.StartDate.ToAcademicYear(), command.StartDate.ToDeliveryPeriod(), monthlyAmount, MathsAndEnglishInstalmentType.Regular));
        

        // Remove all instalments if the withdrawal date is before the end of the qualifying period
        if (command.WithdrawalDate.HasValue && !WithdrawnLearnerQualifiesForEarnings(command.StartDate, command.EndDate, command.WithdrawalDate.Value))
            return new MathsAndEnglish(command.StartDate, command.EndDate, command.Course, command.Amount, new List<MathsAndEnglishInstalment>(), command.WithdrawalDate, command.ActualEndDate, command.PriorLearningAdjustmentPercentage);

        return new MathsAndEnglish(command.StartDate, command.EndDate, command.Course, command.Amount, instalments, command.WithdrawalDate, command.ActualEndDate, command.PriorLearningAdjustmentPercentage);
    }

    private static bool WithdrawnLearnerQualifiesForEarnings(DateTime startDate, DateTime endDate, DateTime withdrawalDate)
    {
        var plannedLength = (endDate - startDate).TotalDays + 1;
        var actualLength = (withdrawalDate - startDate).TotalDays + 1;

        if (plannedLength >= 168)
            return actualLength >= 42;
        if (plannedLength >= 14)
            return actualLength >= 14;

        return actualLength >= 1;
    }
}