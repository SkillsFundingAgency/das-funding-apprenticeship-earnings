using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Apprenticeship;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Extensions;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Calculations;

public static class BalancingInstalments
{
    public static List<Instalment> GenerateInstalmentsBalancedForCompletion(DateTime completionDate, decimal completionAmount, List<Instalment> instalments)
    {
        var completionPeriod = completionDate.ToDeliveryPeriod();
        var completionYear = completionDate.ToAcademicYear();

        var completionInstalment = instalments.SingleOrDefault(x => x.AcademicYear == completionYear && x.DeliveryPeriod == completionPeriod);

        //If there is no instalment for the completion period, it's after the current price episodes/end date so no balancing required
        if (completionInstalment == null)
        {
            return instalments;
        }

        //Calculate the balancing amount
        var balancingAmount = 0m;

        foreach (var instalment in instalments)
        {
            if (instalment.AcademicYear < completionYear
                || (instalment.AcademicYear == completionYear && instalment.DeliveryPeriod < completionPeriod))
            {
                balancingAmount += instalment.Amount;
            }
        }

        //Remove all instalments before and on the completion date
        instalments.RemoveAll(x =>
            x.AcademicYear < completionYear || (x.AcademicYear == completionYear && x.DeliveryPeriod <= completionPeriod));

        //Now create balancing instalment
        if (balancingAmount > 0)
        {
            var balancingInstalment = new Instalment(completionYear, completionPeriod, balancingAmount, completionInstalment.EpisodePriceKey, InstalmentType.Balancing);
            instalments.Add(balancingInstalment);
        }

        return instalments;
    }
}