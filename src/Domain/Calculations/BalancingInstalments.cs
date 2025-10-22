using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Apprenticeship;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Extensions;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Calculations;

public static class BalancingInstalments
{
    public static List<Instalment> BalanceInstalmentsForCompletion(DateTime completionDate, List<Instalment> instalments, DateTime plannedEndDate)
    {
        var completionPeriod = completionDate.ToDeliveryPeriod();
        var completionYear = completionDate.ToAcademicYear();

        var completionPeriodInstalment = instalments.SingleOrDefault(x => x.AcademicYear == completionYear && x.DeliveryPeriod == completionPeriod);

        var nextPeriod = completionDate.LastDayOfMonth().AddDays(1).ToDeliveryPeriod();
        var nextPeriodYear = completionDate.LastDayOfMonth().AddDays(1).ToAcademicYear();
        var completionOnTime = !instalments.Any(x => x.AcademicYear == nextPeriodYear && x.DeliveryPeriod == nextPeriod) && completionDate >= plannedEndDate;

        //No balancing is required if either:
        // there is no instalment for the completion period (it's after the current price episodes/end date)
        // or the learner completed on time
        if (completionPeriodInstalment == null || completionOnTime)
        {
            return instalments;
        }

        //Calculate the balancing amount
        var balancingAmount = 0m;

        foreach (var instalment in instalments)
        {
            if (instalment.AcademicYear > completionYear
                || (instalment.AcademicYear == completionYear && instalment.DeliveryPeriod >= completionPeriod))
            {
                balancingAmount += instalment.Amount;
            }
        }

        //Remove all instalments after and on the completion date
        instalments.RemoveAll(x =>
            x.AcademicYear > completionYear || (x.AcademicYear == completionYear && x.DeliveryPeriod >= completionPeriod));

        //Now create balancing instalment
        if (balancingAmount > 0)
        {
            var balancingInstalment = new Instalment(completionYear, completionPeriod, balancingAmount, completionPeriodInstalment.EpisodePriceKey, InstalmentType.Balancing);
            instalments.Add(balancingInstalment);
        }

        return instalments;
    }
}