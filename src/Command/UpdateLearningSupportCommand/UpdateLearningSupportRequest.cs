using SFA.DAS.Funding.ApprenticeshipEarnings.Types;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.Command.UpdateLearningSupportCommand;

public class UpdateLearningSupportRequest
{
    public List<LearningSupportItem> LearningSupport { get; set; } = [];
}