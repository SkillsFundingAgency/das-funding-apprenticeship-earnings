using AutoFixture;
using FluentAssertions;
using Moq;
using NServiceBus;
using SFA.DAS.Funding.ApprenticeshipEarnings.Command.UpdateOnProgrammeCommand;
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

namespace SFA.DAS.Funding.ApprenticeshipEarnings.Command.UnitTests.UpdateOnProgrammeCommandHandler;

[TestFixture]
public class WhenSavePrices
{
    private readonly Fixture _fixture = new();
    private Mock<IApprenticeshipRepository> _mockApprenticeshipRepository;
    private Mock<ISystemClockService> _mockSystemClock;
    private UpdateOnProgrammeCommand.UpdateOnProgrammeCommandHandler _handler;

    [SetUp]
    public void SetUp()
    {
        _mockApprenticeshipRepository = new Mock<IApprenticeshipRepository>();
        _mockSystemClock = new Mock<ISystemClockService>();

        _handler = new UpdateOnProgrammeCommand.UpdateOnProgrammeCommandHandler(
            _mockApprenticeshipRepository.Object,
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

    private UpdateOnProgrammeCommand.UpdateOnProgrammeCommand BuildCommand(Apprenticeship apprenticeship)
    {
        var currentEpisode = apprenticeship.ApprenticeshipEpisodes.First();
        var saveRequest = new UpdateOnProgrammeRequest
        {
            ApprenticeshipEpisodeKey = currentEpisode.ApprenticeshipEpisodeKey,
            FundingBandMaximum = int.MaxValue,
            IncludesFundingBandMaximumUpdate = true,
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
                TotalPrice = 5500
            });
        }

        return new UpdateOnProgrammeCommand.UpdateOnProgrammeCommand(apprenticeship.ApprenticeshipKey, saveRequest);
    }
}