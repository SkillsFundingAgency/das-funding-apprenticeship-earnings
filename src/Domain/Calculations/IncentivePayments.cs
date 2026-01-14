using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Apprenticeship;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.ApprenticeshipFunding;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Extensions;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Calculations;

public static class IncentivePayments
{
    public static List<IncentivePayment> GenerateIncentivePayments(int ageAtStartOfLearning, DateTime apprenticeshipStartDate, DateTime apprenticeshipEndDate, bool hasEHCP, bool isCareLeaver, bool careLeaverEmployerConsentGiven, List<EpisodePeriodInLearning> periodsInLearning)
    {
        //todo this class needs to handle PIL instead of BIL
        var incentivePayments = new List<IncentivePayment>();
        incentivePayments.AddRange(GenerateUnder19sIncentivePayments(ageAtStartOfLearning, apprenticeshipStartDate, apprenticeshipEndDate, periodsInLearning));
        incentivePayments.AddRange(Generate19To24IncentivePayments(ageAtStartOfLearning, apprenticeshipStartDate, apprenticeshipEndDate, hasEHCP, isCareLeaver, careLeaverEmployerConsentGiven, periodsInLearning));
        return incentivePayments;
    }

    public static List<IncentivePayment> GenerateUnder19sIncentivePayments(int ageAtStartOfLearning, DateTime apprenticeshipStartDate, DateTime apprenticeshipEndDate, List<EpisodePeriodInLearning> periodsInLearning)
    {
        var incentivePayments = new List<IncentivePayment>();

        if (ageAtStartOfLearning is > 18)
        {
            return incentivePayments;
        }

        if(IsEligibleForIncentive(apprenticeshipStartDate, apprenticeshipEndDate, 89, periodsInLearning))
        {
            var incentiveDate = AdjustForBreaks(apprenticeshipStartDate, 89, periodsInLearning);
            incentivePayments.AddIncentivePayment(incentiveDate, AdditionalPaymentAmounts.Incentive, InstalmentTypes.ProviderIncentive);
            incentivePayments.AddIncentivePayment(incentiveDate, AdditionalPaymentAmounts.Incentive, InstalmentTypes.EmployerIncentive);
        }

        if(IsEligibleForIncentive(apprenticeshipStartDate, apprenticeshipEndDate, 364, periodsInLearning))
        {
            var incentiveDate = AdjustForBreaks(apprenticeshipStartDate, 364, periodsInLearning);
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
        List<EpisodePeriodInLearning> periodsInLearning)
    {
        var incentivePayments = new List<IncentivePayment>();

        // Does not fit age criteria
        if (ageAtStartOfLearning is < 19 || ageAtStartOfLearning is > 24)
            return incentivePayments;

        // Is not eligible for incentive payments
        if (!hasEHCP && !isCareLeaver)
            return incentivePayments;

        if (IsEligibleForIncentive(apprenticeshipStartDate, apprenticeshipEndDate, 89, periodsInLearning))
        {
            var incentiveDate = AdjustForBreaks(apprenticeshipStartDate, 89, periodsInLearning);
            incentivePayments.AddIncentivePayment(incentiveDate, AdditionalPaymentAmounts.Incentive, InstalmentTypes.ProviderIncentive);

            if ((careLeaverEmployerConsentGiven && isCareLeaver) || hasEHCP)
            {
                incentivePayments.AddIncentivePayment(incentiveDate, AdditionalPaymentAmounts.Incentive, InstalmentTypes.EmployerIncentive);
            }
        }

        if (IsEligibleForIncentive(apprenticeshipStartDate, apprenticeshipEndDate, 364, periodsInLearning))
        {
            var incentiveDate = AdjustForBreaks(apprenticeshipStartDate, 364, periodsInLearning);
            incentivePayments.AddIncentivePayment(incentiveDate, AdditionalPaymentAmounts.Incentive, InstalmentTypes.ProviderIncentive);

            if ((careLeaverEmployerConsentGiven && isCareLeaver) || hasEHCP)
            {
                incentivePayments.AddIncentivePayment(incentiveDate, AdditionalPaymentAmounts.Incentive, InstalmentTypes.EmployerIncentive);
            }
        }

        return incentivePayments;
    }

    private static bool IsEligibleForIncentive(DateTime startDate, DateTime endDate, int milestoneDays, List<EpisodePeriodInLearning> periodsInLearning)
    {
        var adjustedDate = AdjustForBreaks(startDate, milestoneDays, periodsInLearning);
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

    private static DateTime AdjustForBreaks(DateTime startDate, int milestoneDays, List<EpisodePeriodInLearning> periodsInLearning)
    {
        var incentiveDate = startDate.AddDays(milestoneDays);

        var orderedPeriods = periodsInLearning
            .OrderBy(x => x.StartDate)
            .ToList();

        for (var i = 0; i < orderedPeriods.Count; i++)
        {
            var periodInLearning = orderedPeriods[i];

            // Case 1: break started before incentive date, with a return recorded
            if (periodInLearning.EndDate < incentiveDate && i + 1 < orderedPeriods.Count)
                incentiveDate = incentiveDate.AddDays(periodInLearning.GetBreakDurationUntilNextPeriod(orderedPeriods[i + 1]));

            // Case 2: break started before incentive date, with no return recorded
            if (periodInLearning.EndDate < incentiveDate && i + 1 >= orderedPeriods.Count)
                incentiveDate = incentiveDate.AddDays((periodInLearning.OriginalExpectedEndDate - periodInLearning.EndDate).Days - 1);

            // Case 3: break starts after the current incentive date - no effect
        }

        return incentiveDate;
    }
}
