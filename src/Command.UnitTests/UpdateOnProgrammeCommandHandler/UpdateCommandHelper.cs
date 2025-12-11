using SFA.DAS.Learning.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.Command.UnitTests.UpdateOnProgrammeCommandHandler;

internal static class UpdateCommandHelper
{
    internal static UpdateOnProgrammeCommand.UpdateOnProgrammeCommand BuildCommand(Domain.Apprenticeship.Apprenticeship apprenticeship, int fundingBand = 0)
    {
        var episode = apprenticeship.ApprenticeshipEpisodes.Single();

        var request = new UpdateOnProgrammeCommand.UpdateOnProgrammeRequest
        {
            ApprenticeshipEpisodeKey = episode.ApprenticeshipEpisodeKey,
            CompletionDate = episode.CompletionDate,
            WithdrawalDate = episode.WithdrawalDate,
            PauseDate = episode.PauseDate,
            DateOfBirth = apprenticeship.DateOfBirth,
            //BreaksInLearning = do later
            Prices = GetPrices(episode)
        };

        if (fundingBand > 0)
        {
            request.FundingBandMaximum = fundingBand;
            request.IncludesFundingBandMaximumUpdate = true;
        }

        var command = new UpdateOnProgrammeCommand.UpdateOnProgrammeCommand(apprenticeship.ApprenticeshipKey, request);

        return command;
    }

    private static List<LearningEpisodePrice> GetPrices(Domain.Apprenticeship.ApprenticeshipEpisode episode)
    {
        var prices = episode.Prices.Select(p => new LearningEpisodePrice
        {
            Key = p.PriceKey,
            StartDate = p.StartDate,
            EndDate = p.EndDate,
            TotalPrice = p.AgreedPrice,
            EndPointAssessmentPrice = p.AgreedPrice * 0.2m,
            TrainingPrice = p.AgreedPrice * 0.8m
        }).ToList();

        return prices;
    }
}
