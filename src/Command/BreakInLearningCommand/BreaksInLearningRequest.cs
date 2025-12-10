namespace SFA.DAS.Funding.ApprenticeshipEarnings.Command.BreakInLearningCommand;

#pragma warning disable CS8618

public class BreaksInLearningRequest
{
    public Guid EpisodeKey { get; set; }
    public List<BreakInLearningPeriod> BreaksInLearning { get; set; }
}

public class BreakInLearningPeriod
{
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public DateTime PriorPeriodExpectedEndDate { get; set; }
}
#pragma warning restore CS8618