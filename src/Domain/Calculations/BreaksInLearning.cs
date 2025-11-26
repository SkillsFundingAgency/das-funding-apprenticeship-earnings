using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Apprenticeship;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Extensions;


namespace SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Calculations;

internal static class BreaksInLearning
{
    internal static List<Instalment> RecalculateInstalments(List<Instalment> instalments, IEnumerable<EpisodeBreakInLearning> breaksInLearning)
    {
        GuardClause(instalments);

        var instalmentsTotal = instalments.Sum(i => i.Amount);
        var instalmentCount = instalments.Count;
        var updatedInstalments = new List<Instalment>();

        foreach (var instalment in instalments.OrderBy(i => i.DeliveryPeriod.GetCensusDate(i.AcademicYear)))
        {
            if (IsInstalmentDueDuringBreak(instalment, breaksInLearning))
            {
                // This instalment falls within a break in learning, so do not include it and its amount
                // should not be counted towards the total
            }
            else
            {
                var instalmentAmount = instalmentsTotal / instalmentCount;

                updatedInstalments.Add(new Instalment(
                    instalment.AcademicYear, 
                    instalment.DeliveryPeriod,
                    instalmentAmount,
                    instalment.EpisodePriceKey));

                instalmentsTotal = instalmentsTotal - instalmentAmount;
            }

            instalmentCount--;
        }

        return updatedInstalments;
    }

    private static void GuardClause(List<Instalment> instalments)
    {
        if (instalments.Any(x => x.Type != InstalmentType.Regular))
        {
            // Guard clause: RecalculateInstalments must only run against regular instalments.
            // Other instalment types *do* exist in the system, but they are added
            // later in the process. This method intentionally runs before those
            // types are introduced. 
            //
            // If a future change calls this method after non-regular types have been
            // added, the recalculation logic will become incorrect. This exception
            // is here to catch that misuse early and protect the integrity of the
            // calculation flow.
            throw new InvalidOperationException("Can only recalculate regular instalments");
        }
    }

    private static bool IsInstalmentDueDuringBreak(Instalment instalment, IEnumerable<EpisodeBreakInLearning> breaksInLearning)
    {
        var censusDate = instalment.DeliveryPeriod.GetCensusDate(instalment.AcademicYear);

        foreach (var breakInLearning in breaksInLearning)
        {
            if (censusDate >= breakInLearning.StartDate && censusDate <= breakInLearning.EndDate)
            {
                return true;
            }
        }

        return false;
    }
}
