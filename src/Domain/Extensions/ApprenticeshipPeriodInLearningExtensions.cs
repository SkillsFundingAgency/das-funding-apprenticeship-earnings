using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Calculations;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Models.Apprenticeship;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Extensions;

public static class ApprenticeshipPeriodInLearningExtensions
{
    public static int GetBreakDurationUntilNextPeriod(this ApprenticeshipPeriodInLearning period, ApprenticeshipPeriodInLearning nextPeriod)
    {
        return (nextPeriod.StartDate - period.EndDate).Days - 1;
    }

    public static bool QualifiesForEarnings(this ApprenticeshipPeriodInLearning period, DateTime lastDayOfLearning)
    {
        var qDays = QualifyingPeriod.GetQualifyingPeriodDays(period.StartDate, period.OriginalExpectedEndDate);
        var effectiveEndDate = period.EndDate < lastDayOfLearning ? period.EndDate : lastDayOfLearning;

        return effectiveEndDate >= period.StartDate.AddDays(qDays - 1);
    }

    public static bool SpansDeliveryPeriod(this ApprenticeshipPeriodInLearning period, short academicYear, byte deliveryPeriod, DateTime lastDayOfLearning)
    {
        var instalmentMonth = new DateTime(academicYear.ToCalendarYear(deliveryPeriod), deliveryPeriod.ToCalendarMonth(), 1);
        var effectiveEndDate = period.EndDate < lastDayOfLearning ? period.EndDate : lastDayOfLearning;
        var pStartMonth = new DateTime(period.StartDate.Year, period.StartDate.Month, 1);
        var pEndMonth = new DateTime(effectiveEndDate.Year, effectiveEndDate.Month, 1);

        return instalmentMonth >= pStartMonth && instalmentMonth <= pEndMonth;
    }
}