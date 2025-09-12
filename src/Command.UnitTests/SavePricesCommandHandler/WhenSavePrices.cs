using AutoFixture;
using FluentAssertions;
using Moq;
using NServiceBus;
using SFA.DAS.Funding.ApprenticeshipEarnings.Command.ProcessUpdatedEpisodeCommand;
using SFA.DAS.Funding.ApprenticeshipEarnings.Command.SavePricesCommand;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Apprenticeship;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Repositories;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Services;
using SFA.DAS.Funding.ApprenticeshipEarnings.Types;
using SFA.DAS.Learning.Enums;
using SFA.DAS.Learning.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.Command.UnitTests.SavePricesCommandHandler;

[TestFixture]
public class WhenSavePrices
{
    private readonly Fixture _fixture = new();
    private Mock<IApprenticeshipRepository> _mockApprenticeshipRepository;
    private Mock<IMessageSession> _mockMessageSession;
    private Mock<IApprenticeshipEarningsRecalculatedEventBuilder> _mockEventBuilder;
    private Mock<ISystemClockService> _mockSystemClock;
    private SavePricesCommand.SavePricesCommandHandler _handler;

    [SetUp]
    public void SetUp()
    {
        _mockApprenticeshipRepository = new Mock<IApprenticeshipRepository>();
        _mockMessageSession = new Mock<IMessageSession>();
        _mockEventBuilder = new Mock<IApprenticeshipEarningsRecalculatedEventBuilder>();
        _mockSystemClock = new Mock<ISystemClockService>();

        _mockEventBuilder
            .Setup(x => x.Build(It.IsAny<Apprenticeship>()))
            .Returns(new ApprenticeshipEarningsRecalculatedEvent());

        _handler = new SavePricesCommand.SavePricesCommandHandler(
            _mockApprenticeshipRepository.Object,
            _mockMessageSession.Object,
            _mockEventBuilder.Object,
            _mockSystemClock.Object);

    }

    [Test]
    public async Task Handle_ShouldCallRepositoryGet_WithApprenticeshipKey()
    {
        // Arrange
        var apprenticeship = _fixture.BuildApprenticeship();
        var command = BuildCommand(apprenticeship);
        _mockApprenticeshipRepository
            .Setup(repo => repo.Get(command.ApprenticeshipKey))
            .ReturnsAsync(apprenticeship);

        // Act
        await _handler.Handle(command);

        // Assert
        _mockApprenticeshipRepository.Verify(repo => repo.Get(command.ApprenticeshipKey), Times.Once);
    }

    [Test]
    public async Task Handle_ShouldUpdatePrices_OnApprenticeship()
    {
        // Note we can't actually verify the update prices was called, so we are just checking that the apprenticeship
        // object has changed after the call, other test validate the logic within the domain model

        // Arrange
        var apprenticeship = _fixture.BuildApprenticeship();
        var priceBeforeUpdate = apprenticeship.ApprenticeshipEpisodes.First().Prices.First().AgreedPrice;

        var command = BuildCommand(apprenticeship);
        _mockApprenticeshipRepository
            .Setup(repo => repo.Get(command.ApprenticeshipKey))
            .ReturnsAsync(apprenticeship);

        // Act
        await _handler.Handle(command);

        // Assert
        apprenticeship.ApprenticeshipEpisodes.First().Prices.First().AgreedPrice.Should().NotBe(priceBeforeUpdate);
    }

    [Test]
    public async Task Handle_ShouldCallRepositoryUpdate_WithUpdatedApprenticeship()
    {
        // Arrange
        var apprenticeship = _fixture.BuildApprenticeship();
        var command = BuildCommand(apprenticeship);
        _mockApprenticeshipRepository
            .Setup(repo => repo.Get(command.ApprenticeshipKey))
            .ReturnsAsync(apprenticeship);

        // Act
        await _handler.Handle(command);

        // Assert
        _mockApprenticeshipRepository.Verify(repo => repo.Update(apprenticeship), Times.Once);
    }

    [Test]
    public async Task Handle_ShouldPublishEvent_FromEventBuilder()
    {
        // Arrange
        var apprenticeship = _fixture.BuildApprenticeship();
        var command = BuildCommand(apprenticeship);
        var expectedEvent = new ApprenticeshipEarningsRecalculatedEvent();

        _mockApprenticeshipRepository
            .Setup(repo => repo.Get(command.ApprenticeshipKey))
            .ReturnsAsync(apprenticeship);

        _mockEventBuilder
            .Setup(b => b.Build(apprenticeship))
            .Returns(expectedEvent);

        // Act
        await _handler.Handle(command);

        // Assert
        _mockMessageSession.Verify(m => m.Publish(expectedEvent, It.IsAny<PublishOptions>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    private SavePricesCommand.SavePricesCommand BuildCommand(Apprenticeship apprenticeship)
    {
        var currentEpisode = apprenticeship.ApprenticeshipEpisodes.First();
        var saveRequest = new SavePricesRequest
        {
            ApprenticeshipEpisodeKey = currentEpisode.ApprenticeshipEpisodeKey,
            Prices = new List<LearningEpisodePrice>()
        };

        foreach(var existingPrice in currentEpisode.Prices)
        {
            var model = existingPrice.GetModel();
            saveRequest.Prices.Add(new LearningEpisodePrice
            {
                Key = model.Key,
                StartDate = existingPrice.StartDate,
                EndDate = existingPrice.EndDate,
                TrainingPrice = 5000, 
                EndPointAssessmentPrice = 500,
                FundingBandMaximum = 6000,
                TotalPrice = 5500
            });
        }

        return new SavePricesCommand.SavePricesCommand(apprenticeship.ApprenticeshipKey, saveRequest);
    }
}