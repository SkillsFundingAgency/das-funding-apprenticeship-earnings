using AutoFixture;
using Microsoft.Extensions.Logging;
using Moq;
using SFA.DAS.Funding.ApprenticeshipEarnings.Command.UpdateOnProgrammeCommand;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Models;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Models.Apprenticeship;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Repositories;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Services;
using SFA.DAS.Learning.Types;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.Command.UnitTests.UpdateOnProgrammeCommandHandler;

public abstract class BaseUpdateCommandHandlerTests
{
    private readonly Fixture _fixture = new();
    private Mock<ILogger<UpdateOnProgrammeCommand.UpdateOnProgrammeCommandHandler>> _loggerMock;
    private Mock<ILearningRepository> _mockRepository;
    private Mock<ISystemClockService> _mockSystemClockService;

    protected Fixture Fixture => _fixture;
    protected Mock<ILearningRepository> LearningRepositoryMock => _mockRepository;
    protected Mock<ISystemClockService> SystemClockServiceMock => _mockSystemClockService;

    public UpdateOnProgrammeCommand.UpdateOnProgrammeCommandHandler GetUpdateOnProgrammeCommandHandler()
    {
        _loggerMock = new Mock<ILogger<UpdateOnProgrammeCommand.UpdateOnProgrammeCommandHandler>>();
        _mockRepository = new Mock<ILearningRepository>();
        _mockSystemClockService = new Mock<ISystemClockService>();
        _mockSystemClockService.Setup(x => x.UtcNow).Returns(DateTime.UtcNow);

        var handler = new UpdateOnProgrammeCommand.UpdateOnProgrammeCommandHandler(
            _loggerMock.Object,
            _mockRepository.Object,
            _mockSystemClockService.Object);

        return handler;
    }

    internal static UpdateOnProgrammeCommand.UpdateOnProgrammeCommand BuildCommand(Domain.Models.Learning learningDomainModel, int fundingBand = 0)
    {
        var episode = learningDomainModel.ApprenticeshipEpisodes.Single();

        var request = new UpdateOnProgrammeCommand.UpdateOnProgrammeRequest
        {
            ApprenticeshipEpisodeKey = episode.EpisodeKey,
            CompletionDate = episode.CompletionDate,
            WithdrawalDate = episode.WithdrawalDate,
            PauseDate = episode.PauseDate,
            DateOfBirth = learningDomainModel.DateOfBirth,
            Prices = GetPrices(episode),
            Care = new Care
            {
                CareLeaverEmployerConsentGiven = learningDomainModel.CareLeaverEmployerConsentGiven,
                HasEHCP = learningDomainModel.HasEHCP,
                IsCareLeaver = learningDomainModel.IsCareLeaver
            },
            PeriodsInLearning = learningDomainModel.ApprenticeshipEpisodes.SelectMany(e => e.EpisodePeriodsInLearning).Select(p => new PeriodInLearningItem
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

        var command = new UpdateOnProgrammeCommand.UpdateOnProgrammeCommand(learningDomainModel.ApprenticeshipKey, request);

        return command;
    }

    private static List<LearningEpisodePrice> GetPrices(ApprenticeshipEpisode episode)
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
