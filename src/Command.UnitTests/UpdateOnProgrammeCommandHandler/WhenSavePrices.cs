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
public class WhenSavePrices : BaseUpdateCommandHandlerTests
{
    [Test]
    public async Task Handle_ShouldCallRepositoryGet_WithApprenticeshipKey()
    {
        // Arrange
        var apprenticeship = Fixture.BuildApprenticeship();
        var command = BuildCommand(apprenticeship);
        UpdatePrice(command);
        var handler = GetUpdateOnProgrammeCommandHandler();

        ApprenticeshipRepositoryMock
            .Setup(repo => repo.Get(command.ApprenticeshipKey))
            .ReturnsAsync(apprenticeship);

        // Act
        await handler.Handle(command);

        // Assert
        ApprenticeshipRepositoryMock.Verify(repo => repo.Get(command.ApprenticeshipKey), Times.Once);
    }

    [Test]
    public async Task Handle_ShouldUpdatePrices_OnApprenticeship()
    {
        // Arrange
        var apprenticeship = Fixture.BuildApprenticeship();
        var command = BuildCommand(apprenticeship, (int)apprenticeship.ApprenticeshipEpisodes.Single().FundingBandMaximum);
        UpdatePrice(command);

        var priceBeforeUpdate = apprenticeship.ApprenticeshipEpisodes.First().Prices.First().AgreedPrice;
        var initialPriceCount = apprenticeship.ApprenticeshipEpisodes.First().Prices.Count;

        var handler = GetUpdateOnProgrammeCommandHandler();

        ApprenticeshipRepositoryMock
            .Setup(repo => repo.Get(command.ApprenticeshipKey))
            .ReturnsAsync(apprenticeship);

        // Act
        await handler.Handle(command);

        // Assert
        apprenticeship.ApprenticeshipEpisodes.First().Prices.Last().AgreedPrice.Should().NotBe(priceBeforeUpdate);
        apprenticeship.ApprenticeshipEpisodes.First().Prices.Should().HaveCount(initialPriceCount + 1);
    }

    [Test]
    public async Task Handle_ShouldCallRepositoryUpdate_WithUpdatedApprenticeship()
    {
        // Arrange
        var apprenticeship = Fixture.BuildApprenticeship();
        var command = BuildCommand(apprenticeship, (int)apprenticeship.ApprenticeshipEpisodes.Single().FundingBandMaximum);
        UpdatePrice(command);
        var handler = GetUpdateOnProgrammeCommandHandler();
        ApprenticeshipRepositoryMock
            .Setup(repo => repo.Get(command.ApprenticeshipKey))
            .ReturnsAsync(apprenticeship);

        // Act
        await handler.Handle(command);

        // Assert
        ApprenticeshipRepositoryMock.Verify(repo => repo.Update(apprenticeship), Times.Once);
    }

    private void UpdatePrice(UpdateOnProgrammeCommand.UpdateOnProgrammeCommand command)
    {
        var lastPrice = command.Request.Prices.OrderBy(x=>x.StartDate).Last();

        var newPrice = new LearningEpisodePrice
        {
            Key = Guid.NewGuid(),
            StartDate = lastPrice.EndDate.AddDays(-99),
            EndDate = lastPrice.EndDate,
            TotalPrice = lastPrice.TotalPrice + 100,
            TrainingPrice = lastPrice.TrainingPrice + 100,
            EndPointAssessmentPrice = lastPrice.EndPointAssessmentPrice
        };

        lastPrice.EndDate = lastPrice.EndDate.AddDays(-100);

        command.Request.Prices.Add(newPrice);
    }
}