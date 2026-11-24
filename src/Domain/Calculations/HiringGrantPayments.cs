using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.ApprenticeshipFunding;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Extensions;
using SFA.DAS.Funding.ApprenticeshipEarnings.Types;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Calculations;

/// <summary>
/// Employer hiring grant for 16-24 year old apprentices of non-levy employers starting on or after the configured start date (1 October 2026 at launch).
/// Earnings only, these are not sent to payments.
/// </summary>
public static class HiringGrantPayments
{
    private const int MinimumAge = 16;
    private const int MaximumAge = 24;

    // Due on day 90 and day 365 of the apprenticeship (start date + 89 days and + 364 days)
    private static readonly int[] MilestoneDaysAfterStart = [89, 364];

    public static List<IncentivePayment> GenerateHiringGrantPayments(EmployerType employerType, int ageAtStartOfLearning, DateTime apprenticeshipStartDate, DateTime eligibleFromDate)
    {
        if (employerType != EmployerType.NonLevy
            || apprenticeshipStartDate < eligibleFromDate
            || ageAtStartOfLearning < MinimumAge
            || ageAtStartOfLearning > MaximumAge)
        {
            return [];
        }

        return MilestoneDaysAfterStart
            .Select(days => apprenticeshipStartDate.AddDays(days))
            .Select(dueDate => new IncentivePayment
            {
                AcademicYear = dueDate.ToAcademicYear(),
                DeliveryPeriod = dueDate.ToDeliveryPeriod(),
                DueDate = dueDate,
                Amount = AdditionalPaymentAmounts.EmployerHiringGrant,
                IncentiveType = InstalmentTypes.EmployerHiringGrant
            })
            .ToList();
    }
}
