using AutoFixture;
using FluentAssertions;
using Moq;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Models;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Models.Apprenticeship;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Repositories;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Services;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.Command.UnitTests.UpdateOnProgrammeCommandHandler;

[TestFixture]
public class WhenWithdrawUpdated : BaseUpdateCommandHandlerTests
{
    [Test]
    public async Task ThenTheApprenticeshipIsWithdrawn()
    {
        // Arrange
        var learningDomainModel = Fixture.BuildLearning();
        var command = BuildCommand(learningDomainModel);
        command.Request.WithdrawalDate = new DateTime(2024, 11, 30);
        var handler = GetUpdateOnProgrammeCommandHandler();

        SystemClockServiceMock.Setup(x => x.UtcNow).Returns(new DateTime(2024, 12, 1));
        LearningRepositoryMock.Setup(repo => repo.GetApprenticeshipLearning(It.IsAny<Guid>())).ReturnsAsync(learningDomainModel);

        // Act
        await handler.Handle(command);

        // Assert
        LearningRepositoryMock.Verify(x => x.GetApprenticeshipLearning(command.LearningKey), Times.Once);
        LearningRepositoryMock.Verify(x => x.Update(It.IsAny<ApprenticeshipLearning>()), Times.Once);
    }

    [Test]
    public async Task ThenANewEarningsProfileVersionIsGeneratedWhenTheWithdrawalDateIsUpdatedWithinTheSameDeliveryPeriod()
    {
        // Arrange
        var (handler, learningDomainModel, command) = await SetupCalculatedLearning();
        var episode = learningDomainModel.Episodes.Single();
        var startDate = episode.Prices.Min(x => x.StartDate).AddMonths(5);
        var withdrawalDate = new DateTime(startDate.Year, startDate.Month, 10);
        command.Request.WithdrawalDate = withdrawalDate;
        await handler.Handle(command);
        ResetForNextRequest(learningDomainModel);
        var originalVersion = episode.EarningsProfile!.Version;
        var originalInstalments = episode.EarningsProfile.Instalments.Select(x => (x.AcademicYear, x.DeliveryPeriod, x.Amount)).ToList();
        command.Request.WithdrawalDate = withdrawalDate.AddDays(10);

        // Act
        var result = await handler.Handle(command);

        // Assert
        episode.EarningsProfile.Instalments.Select(x => (x.AcademicYear, x.DeliveryPeriod, x.Amount)).Should().BeEquivalentTo(originalInstalments);
        result.HasNewEarningsProfileVersionBeenGenerated.Should().BeTrue();
        episode.EarningsProfile.Version.Should().NotBe(originalVersion);
    }

    [Test]
    public async Task ThenANewEarningsProfileVersionIsNotGeneratedWhenTheWithdrawalDateIsUnchanged()
    {
        // Arrange
        var (handler, learningDomainModel, command) = await SetupCalculatedLearning();
        var episode = learningDomainModel.Episodes.Single();
        var originalVersion = episode.EarningsProfile!.Version;

        // Act
        var result = await handler.Handle(command);

        // Assert
        result.HasNewEarningsProfileVersionBeenGenerated.Should().BeFalse();
        episode.EarningsProfile.Version.Should().Be(originalVersion);
    }

    /// <summary>
    /// Builds a learning with no withdrawal, completion or pause and calculates its earnings, so subsequent
    /// requests only generate a new earnings profile version if something has changed.
    /// </summary>
    private async Task<(UpdateOnProgrammeCommand.UpdateOnProgrammeCommandHandler Handler, ApprenticeshipLearning Learning, UpdateOnProgrammeCommand.UpdateOnProgrammeCommand Command)> SetupCalculatedLearning()
    {
        var learningEntity = Fixture.BuildLearningModel();
        var episodeEntity = learningEntity.Episodes.Single();
        episodeEntity.WithdrawalDate = null;
        episodeEntity.CompletionDate = null;
        episodeEntity.AchievementDate = null;
        episodeEntity.PauseDate = null;

        var learningDomainModel = ApprenticeshipLearning.Get(learningEntity);
        var command = BuildCommand(learningDomainModel);
        var handler = GetUpdateOnProgrammeCommandHandler();
        LearningRepositoryMock.Setup(repo => repo.GetApprenticeshipLearning(It.IsAny<Guid>())).ReturnsAsync(learningDomainModel);

        await handler.Handle(command);
        ResetForNextRequest(learningDomainModel);

        return (handler, learningDomainModel, command);
    }

    /// <summary>
    /// Each request loads the learning afresh, so clear any state left over from a previous request on the same instance.
    /// </summary>
    private static void ResetForNextRequest(ApprenticeshipLearning learning)
    {
        learning.FlushEvents();
        learning.Episodes.Single().HasEarningsProfileVersionUpdate = false;
    }
}
