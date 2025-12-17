using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Apprenticeship;
using SFA.DAS.Learning.Types;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.Command.UpdateOnProgrammeCommand;

public class UpdateOnProgrammeRequest
{
    public Guid ApprenticeshipEpisodeKey { get; set; }
    public DateTime? CompletionDate { get; set; }
    public DateTime? WithdrawalDate { get; set; }
    public DateTime? PauseDate { get; set; }
    public DateTime DateOfBirth { get; set; }
    public int? FundingBandMaximum { get; set; }
    public bool IncludesFundingBandMaximumUpdate { get; set; }
    public List<LearningEpisodePrice> Prices { get; set; } = [];
    public List<BreakInLearningItem> BreaksInLearning { get; set; } = [];
}

public class BreakInLearningItem
{
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public DateTime PriorPeriodExpectedEndDate { get; set; }
}

public static class UpdateOnProgrammeRequestExtensions
{
    public static List<EpisodeBreakInLearning> ToEpisodeBreaksInLearning(this UpdateOnProgrammeRequest request)
    {
        return request.BreaksInLearning
            .Select(b => new EpisodeBreakInLearning(request.ApprenticeshipEpisodeKey, b.StartDate, b.EndDate, b.PriorPeriodExpectedEndDate))
            .ToList(); 
    }
}