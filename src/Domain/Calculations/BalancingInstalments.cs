using SFA.DAS.Funding.ApprenticeshipEarnings.DataAccess.Entities.Apprenticeship;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Extensions;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Models;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Models.Apprenticeship;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Calculations;

public static class BalancingInstalments
{
    public static List<ApprenticeshipInstalment> BalanceInstalmentsForCompletion(DateTime completionDate, List<ApprenticeshipInstalment> instalments, ApprenticeshipEpisodePriceEntity lastEpisodePrice, IReadOnlyCollection<ApprenticeshipPeriodInLearning> periodsInLearning, decimal onProgramTotal)
    {
        var completionPeriod = completionDate.ToDeliveryPeriod();
        var completionYear = completionDate.ToAcademicYear();

        var nextPeriod = completionDate.LastDayOfMonth().AddDays(1).ToDeliveryPeriod();
        var nextPeriodYear = completionDate.LastDayOfMonth().AddDays(1).ToAcademicYear();
        var completionOnTime = !instalments.Any(x => x.AcademicYear == nextPeriodYear && x.DeliveryPeriod == nextPeriod) && completionDate >= lastEpisodePrice.EndDate;

        var lastPeriodHasRegularInstalment = instalments.Any(x => x.Type == InstalmentType.Regular && x.AcademicYear == completionYear && x.DeliveryPeriod == completionPeriod);

        var pastInstalmentsTotal = instalments
            .Where(x => x.AcademicYear < completionYear || (x.AcademicYear == completionYear && x.DeliveryPeriod < completionPeriod))
            .Sum(x => x.Amount);

        var totalAmountIncludingCompletionPeriodRegular = instalments
            .Where(x => x.AcademicYear < completionYear || (x.AcademicYear == completionYear && x.DeliveryPeriod <= completionPeriod))
            .Sum(x => x.Amount);

        //No balancing is required if either:
        // the learner completed on time and has a regular instalment for the last period (because they qualified for earnings and passed a census date for that period)
        // AND the regular instalment already perfectly covers the remaining amount.
        if (completionOnTime && lastPeriodHasRegularInstalment && totalAmountIncludingCompletionPeriodRegular == onProgramTotal)
        {
            return instalments;
        }

        //Calculate the balancing amount
        var balancingAmount = onProgramTotal - pastInstalmentsTotal;

        //Remove all instalments after and on the completion date
        instalments.RemoveAll(x =>
            (x.AcademicYear > completionYear || (x.AcademicYear == completionYear && x.DeliveryPeriod >= completionPeriod)) && x.Type == InstalmentType.Regular);

        //Now create balancing instalment
        if (balancingAmount > 0)
        {
            var balancingInstalment = new ApprenticeshipInstalment(completionYear, completionPeriod, balancingAmount, lastEpisodePrice.Key, InstalmentType.Balancing);
            instalments.Add(balancingInstalment);
        }

        return instalments;
    }
}