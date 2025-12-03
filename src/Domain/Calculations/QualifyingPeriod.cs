namespace SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Calculations;

internal static class QualifyingPeriod
{
    internal static int GetQualifyingPeriodDays(DateTime startDate, DateTime plannedEndDate)
    {
        var plannedDuration = (int)Math.Floor((plannedEndDate - startDate).TotalDays) + 1;
        return plannedDuration switch
        {
            >= 168 => 42,
            >= 14 => 14,
            _ => 1
        };
    }

}
