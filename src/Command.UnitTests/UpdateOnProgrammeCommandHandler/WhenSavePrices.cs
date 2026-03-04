using AutoFixture;
using FluentAssertions;
using Moq;
using NServiceBus;
using SFA.DAS.Funding.ApprenticeshipEarnings.Command.UpdateOnProgrammeCommand;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Repositories;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Services;
using SFA.DAS.Funding.ApprenticeshipEarnings.Types;
using SFA.DAS.Learning.Enums;
using SFA.DAS.Learning.Types;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.Command.UnitTests.UpdateOnProgrammeCommandHandler;

[TestFixture]
public class WhenSavePrices : BaseUpdateCommandHandlerTests
{
    [Test]
    public async Task Handle_ShouldCallRepositoryGet_WithApprenticeshipKey()
    {
        // Arrange
        var learningDomainModel = Fixture.BuildLearning();
        var command = BuildCommand(learningDomainModel);
        UpdatePrice(command);
        var handler = GetUpdateOnProgrammeCommandHandler();

        LearningRepositoryMock
            .Setup(repo => repo.Get(command.ApprenticeshipKey))
            .ReturnsAsync(learningDomainModel);

        // Act
        await handler.Handle(command);

        // Assert
        LearningRepositoryMock.Verify(repo => repo.Get(command.ApprenticeshipKey), Times.Once);
    }

    [Test]
    public async Task Handle_ShouldUpdatePrices_OnApprenticeship()
    {
        // Arrange
        var learningDomainModel = Fixture.BuildLearning();
        var command = BuildCommand(learningDomainModel, (int)learningDomainModel.ApprenticeshipEpisodes.Single().FundingBandMaximum);
        UpdatePrice(command);

        var priceBeforeUpdate = learningDomainModel.ApprenticeshipEpisodes.First().Prices.First().AgreedPrice;
        var initialPriceCount = learningDomainModel.ApprenticeshipEpisodes.First().Prices.Count;

        var handler = GetUpdateOnProgrammeCommandHandler();

        LearningRepositoryMock
            .Setup(repo => repo.Get(command.ApprenticeshipKey))
            .ReturnsAsync(learningDomainModel);

        // Act
        await handler.Handle(command);

        // Assert
        learningDomainModel.ApprenticeshipEpisodes.First().Prices.Last().AgreedPrice.Should().NotBe(priceBeforeUpdate);
        learningDomainModel.ApprenticeshipEpisodes.First().Prices.Should().HaveCount(initialPriceCount + 1);
    }

    [Test]
    public async Task Handle_ShouldCallRepositoryUpdate_WithUpdatedApprenticeship()
    {
        // Arrange
        var learningDomainModel = Fixture.BuildLearning();
        var command = BuildCommand(learningDomainModel, (int)learningDomainModel.ApprenticeshipEpisodes.Single().FundingBandMaximum);
        UpdatePrice(command);
        var handler = GetUpdateOnProgrammeCommandHandler();
        LearningRepositoryMock
            .Setup(repo => repo.Get(command.ApprenticeshipKey))
            .ReturnsAsync(learningDomainModel);

        // Act
        await handler.Handle(command);

        // Assert
        LearningRepositoryMock.Verify(repo => repo.Update(learningDomainModel), Times.Once);
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