namespace SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Apprenticeship;

public static class EpisodePeriodInLearningExtensions
{
    public static int GetBreakDurationUntilNextPeriod(this EpisodePeriodInLearning period, EpisodePeriodInLearning nextPeriod)
    {
        return (nextPeriod.StartDate - period.EndDate).Days - 1;
    }
}