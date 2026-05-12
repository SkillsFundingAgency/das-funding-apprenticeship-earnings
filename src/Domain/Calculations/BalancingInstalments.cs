using SFA.DAS.Funding.ApprenticeshipEarnings.DataAccess.Entities.Apprenticeship;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Extensions;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Models;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Models.Apprenticeship;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Calculations;

public static class BalancingInstalments
{
    public static List<ApprenticeshipInstalment> BalanceInstalmentsForCompletion(DateTime completionDate, List<ApprenticeshipInstalment> instalments, ApprenticeshipEpisodePriceEntity lastEpisodePrice, IReadOnlyCollection<ApprenticeshipPeriodInLearning> periodsInLearning)
    {
        var completionPeriod = completionDate.ToDeliveryPeriod();
        var completionYear = completionDate.ToAcademicYear();

        var nextPeriod = completionDate.LastDayOfMonth().AddDays(1).ToDeliveryPeriod();
        var nextPeriodYear = completionDate.LastDayOfMonth().AddDays(1).ToAcademicYear();
        var completionOnTime = !instalments.Any(x => x.AcademicYear == nextPeriodYear && x.DeliveryPeriod == nextPeriod) && completionDate >= lastEpisodePrice.EndDate;

        var lastPeriodHasRegularInstalment = instalments.Any(x => x.Type == InstalmentType.Regular && x.AcademicYear == completionYear && x.DeliveryPeriod == completionPeriod);

        //No balancing is required if either:
        // the learner completed on time and has a regular instalment for the last period (because they qualified for earnings and passed a census date for that period)
        if (completionOnTime && lastPeriodHasRegularInstalment)
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
            var balancingInstalment = new ApprenticeshipInstalment(completionYear, completionPeriod, balancingAmount, lastEpisodePrice.Key, InstalmentType.Balancing);
            instalments.Add(balancingInstalment);
        }

        return instalments;
    }
}