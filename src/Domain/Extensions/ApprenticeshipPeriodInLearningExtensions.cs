using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Models.Apprenticeship;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Extensions;

public static class ApprenticeshipPeriodInLearningExtensions
{
    public static int GetBreakDurationUntilNextPeriod(this ApprenticeshipPeriodInLearning period, ApprenticeshipPeriodInLearning nextPeriod)
    {
        return (nextPeriod.StartDate - period.EndDate).Days - 1;
    }
}