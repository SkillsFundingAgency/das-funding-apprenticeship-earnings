using SFA.DAS.Learning.Types;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.Command.SavePricesCommand;

public class SavePricesCommand : ICommand
{
    public Guid ApprenticeshipKey { get; }

    public Guid ApprenticeshipEpisodeKey { get; }

    public List<LearningEpisodePrice> Prices { get; }

    public int AgeAtStartOfLearning { get; set; }
    public int FundingBandMaximum { get; set; }

    public SavePricesCommand(Guid apprenticeshipKey, SavePricesRequest savePricesRequest)
    {
        ApprenticeshipKey = apprenticeshipKey;
        ApprenticeshipEpisodeKey = savePricesRequest.ApprenticeshipEpisodeKey;
        Prices = savePricesRequest.Prices;
        AgeAtStartOfLearning = savePricesRequest.AgeAtStartOfLearning;
        FundingBandMaximum = savePricesRequest.FundingBandMaximum;
    }
}
