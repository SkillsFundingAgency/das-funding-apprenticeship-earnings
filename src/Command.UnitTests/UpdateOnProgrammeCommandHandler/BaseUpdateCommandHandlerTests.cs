using AutoFixture;
using Microsoft.Extensions.Logging;
using Moq;
using SFA.DAS.Funding.ApprenticeshipEarnings.Command.UpdateOnProgrammeCommand;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Repositories;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Services;
using SFA.DAS.Learning.Types;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.Command.UnitTests.UpdateOnProgrammeCommandHandler;

public abstract class BaseUpdateCommandHandlerTests
{
    private readonly Fixture _fixture = new();
    private Mock<ILogger<UpdateOnProgrammeCommand.UpdateOnProgrammeCommandHandler>> _loggerMock;
    private Mock<IApprenticeshipRepository> _apprenticeshipRepositoryMock;
    private Mock<ISystemClockService> _systemClockServiceMock;

    protected Fixture Fixture => _fixture;
    protected Mock<IApprenticeshipRepository> ApprenticeshipRepositoryMock => _apprenticeshipRepositoryMock;
    protected Mock<ISystemClockService> SystemClockServiceMock => _systemClockServiceMock;

    public UpdateOnProgrammeCommand.UpdateOnProgrammeCommandHandler GetUpdateOnProgrammeCommandHandler()
    {
        _loggerMock = new Mock<ILogger<UpdateOnProgrammeCommand.UpdateOnProgrammeCommandHandler>>();
        _apprenticeshipRepositoryMock = new Mock<IApprenticeshipRepository>();
        _systemClockServiceMock = new Mock<ISystemClockService>();
        _systemClockServiceMock.Setup(x => x.UtcNow).Returns(DateTime.UtcNow);

        var handler = new UpdateOnProgrammeCommand.UpdateOnProgrammeCommandHandler(
            _loggerMock.Object,
            _apprenticeshipRepositoryMock.Object,
            _systemClockServiceMock.Object);

        return handler;
    }

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
            Prices = GetPrices(episode),
            Care = new Care
            {
                CareLeaverEmployerConsentGiven = apprenticeship.CareLeaverEmployerConsentGiven,
                HasEHCP = apprenticeship.HasEHCP,
                IsCareLeaver = apprenticeship.IsCareLeaver
            },
            PeriodsInLearning = apprenticeship.ApprenticeshipEpisodes.SelectMany(e => e.EpisodePeriodsInLearning).Select(p => new PeriodInLearningItem
            {
                StartDate = p.StartDate,
                EndDate = p.EndDate,
                OriginalExpectedEndDate = p.OriginalExpectedEndDate
            }).ToList()
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
