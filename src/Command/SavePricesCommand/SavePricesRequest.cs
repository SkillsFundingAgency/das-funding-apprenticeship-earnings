using SFA.DAS.Learning.Types;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.Command.SavePricesCommand;

public class SavePricesRequest
{

    public Guid ApprenticeshipEpisodeKey { get; set; }

    public List<LearningEpisodePrice> Prices { get; set; } = new();
    public int AgeAtStartOfLearning { get; set; }
    public int FundingBandMaximum { get; set; }
}
