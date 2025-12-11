using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Apprenticeship;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.ApprenticeshipFunding;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Extensions;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Calculations;

public static class IncentivePayments
{
    public static List<IncentivePayment> GenerateIncentivePayments(int ageAtStartOfLearning, DateTime apprenticeshipStartDate, DateTime apprenticeshipEndDate, bool hasEHCP, bool isCareLeaver, bool careLeaverEmployerConsentGiven, List<EpisodeBreakInLearning> breaksInLearning)
    {
        var incentivePayments = new List<IncentivePayment>();
        incentivePayments.AddRange(GenerateUnder19sIncentivePayments(ageAtStartOfLearning, apprenticeshipStartDate, apprenticeshipEndDate, breaksInLearning));
        incentivePayments.AddRange(Generate19To24IncentivePayments(ageAtStartOfLearning, apprenticeshipStartDate, apprenticeshipEndDate, hasEHCP, isCareLeaver, careLeaverEmployerConsentGiven, breaksInLearning));
        return incentivePayments;
    }

    public static List<IncentivePayment> GenerateUnder19sIncentivePayments(int ageAtStartOfLearning, DateTime apprenticeshipStartDate, DateTime apprenticeshipEndDate, List<EpisodeBreakInLearning> breaksInLearning)
    {
        var incentivePayments = new List<IncentivePayment>();

        if (ageAtStartOfLearning is > 18)
        {
            return incentivePayments;
        }

        if(IsEligibleForIncentive(apprenticeshipStartDate, apprenticeshipEndDate, 89, breaksInLearning))
        {
            var incentiveDate = AdjustForBreaks(apprenticeshipStartDate, 89, breaksInLearning);
            incentivePayments.AddIncentivePayment(incentiveDate, AdditionalPaymentAmounts.Incentive, InstalmentTypes.ProviderIncentive);
            incentivePayments.AddIncentivePayment(incentiveDate, AdditionalPaymentAmounts.Incentive, InstalmentTypes.EmployerIncentive);
        }

        if(IsEligibleForIncentive(apprenticeshipStartDate, apprenticeshipEndDate, 364, breaksInLearning))
        {
            var incentiveDate = AdjustForBreaks(apprenticeshipStartDate, 364, breaksInLearning);
            incentivePayments.AddIncentivePayment(incentiveDate, AdditionalPaymentAmounts.Incentive, InstalmentTypes.ProviderIncentive);
            incentivePayments.AddIncentivePayment(incentiveDate, AdditionalPaymentAmounts.Incentive, InstalmentTypes.EmployerIncentive);
        }

        return incentivePayments;
    }
    public static List<IncentivePayment> Generate19To24IncentivePayments(
        int ageAtStartOfLearning,
        DateTime apprenticeshipStartDate,
        DateTime apprenticeshipEndDate,
        bool hasEHCP,
        bool isCareLeaver,
        bool careLeaverEmployerConsentGiven,
        List<EpisodeBreakInLearning> breaksInLearning)
    {
        var incentivePayments = new List<IncentivePayment>();

        // Does not fit age criteria
        if (ageAtStartOfLearning is < 19 || ageAtStartOfLearning is > 24)
            return incentivePayments;

        // Is not eligible for incentive payments
        if (!hasEHCP && !isCareLeaver)
            return incentivePayments;

        if (IsEligibleForIncentive(apprenticeshipStartDate, apprenticeshipEndDate, 89, breaksInLearning))
        {
            var incentiveDate = AdjustForBreaks(apprenticeshipStartDate, 89, breaksInLearning);
            incentivePayments.AddIncentivePayment(incentiveDate, AdditionalPaymentAmounts.Incentive, InstalmentTypes.ProviderIncentive);

            if ((careLeaverEmployerConsentGiven && isCareLeaver) || hasEHCP)
            {
                incentivePayments.AddIncentivePayment(incentiveDate, AdditionalPaymentAmounts.Incentive, InstalmentTypes.EmployerIncentive);
            }
        }

        if (IsEligibleForIncentive(apprenticeshipStartDate, apprenticeshipEndDate, 364, breaksInLearning))
        {
            var incentiveDate = AdjustForBreaks(apprenticeshipStartDate, 364, breaksInLearning);
            incentivePayments.AddIncentivePayment(incentiveDate, AdditionalPaymentAmounts.Incentive, InstalmentTypes.ProviderIncentive);

            if ((careLeaverEmployerConsentGiven && isCareLeaver) || hasEHCP)
            {
                incentivePayments.AddIncentivePayment(incentiveDate, AdditionalPaymentAmounts.Incentive, InstalmentTypes.EmployerIncentive);
            }
        }

        return incentivePayments;
    }

    private static bool IsEligibleForIncentive(DateTime startDate, DateTime endDate, int milestoneDays, List<EpisodeBreakInLearning> breaks)
    {
        var adjustedDate = AdjustForBreaks(startDate, milestoneDays, breaks);
        return adjustedDate <= endDate;
    }

    private static void AddIncentivePayment(this List<IncentivePayment> incentivePayments, DateTime dueDate, decimal amount, string incentiveType)
    {
        incentivePayments.Add(new IncentivePayment
        {
            AcademicYear = dueDate.ToAcademicYear(),
            DueDate = dueDate,
            Amount = amount,
            DeliveryPeriod = dueDate.ToDeliveryPeriod(),
            IncentiveType = incentiveType
        });
    }

    private static DateTime AdjustForBreaks(DateTime startDate, int milestoneDays, List<EpisodeBreakInLearning> breaksInLearning)
    {
        var incentiveDate = startDate.AddDays(milestoneDays);
        if (breaksInLearning == null || breaksInLearning.Count == 0)
            return incentiveDate;

        foreach (var b in breaksInLearning.OrderBy(x => x.StartDate))
        {
            // Case 1: break ended before incentive date - extend by its duration
            if (b.EndDate < incentiveDate)
            {
                incentiveDate = incentiveDate.AddDays(b.Duration);
                continue;
            }

            // Case 2: incentive date falls inside the break - carry over the overrun
            if (incentiveDate >= b.StartDate && incentiveDate <= b.EndDate)
            {
                var overrunDays = (incentiveDate - b.StartDate).Days; // how deep we are into the break
                incentiveDate = b.EndDate.AddDays(overrunDays);
            }

            // Case 3: break starts after the current incentive date - no effect, ignore
        }

        return incentiveDate;
    }
}
