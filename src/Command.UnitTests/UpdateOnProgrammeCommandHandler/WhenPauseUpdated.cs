using AutoFixture;
using FluentAssertions;
using Moq;
using SFA.DAS.Funding.ApprenticeshipEarnings.DataAccess.Entities;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Models;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Models.Apprenticeship;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Repositories;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Services;
using SFA.DAS.Funding.ApprenticeshipEarnings.Types;
using SFA.DAS.Learning.Types;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.Command.UnitTests.UpdateOnProgrammeCommandHandler;

[TestFixture]
public class WhenPauseUpdated : BaseUpdateCommandHandlerTests
{

    [Test]
    public async Task Handle_WhenPaused_ShouldSaveUpdatedApprenticeship()
    {
        // Arrange
        var learningDomainModel = Fixture.BuildLearning();
        var command = BuildCommand(learningDomainModel);
        var pauseDate = GetValidPauseDate(learningDomainModel);
        command.Request.PauseDate = pauseDate;

        var handler = GetUpdateOnProgrammeCommandHandler();

        LearningRepositoryMock
            .Setup(r => r.GetApprenticeshipLearning(learningDomainModel.LearningKey))
            .ReturnsAsync(learningDomainModel);

        // Act
        await handler.Handle(command);

        // Assert
        LearningRepositoryMock.Verify(r => r.Update(It.Is<ApprenticeshipLearning>(a => 
            a == learningDomainModel && 
            a.Episodes.First().PauseDate == pauseDate)), Times.Once);
    }

    [Test]
    public async Task Handle_WhenPausingWithInvalidKey_ShouldPropagateExceptionsFromRepository()
    {
        // Arrange
        var learningDomainModel = Fixture.BuildLearning();
        var command = BuildCommand(learningDomainModel);
        var pauseDate = GetValidPauseDate(learningDomainModel);
        command.Request.PauseDate = pauseDate;

        var handler = GetUpdateOnProgrammeCommandHandler();

        LearningRepositoryMock
            .Setup(r => r.GetApprenticeshipLearning(It.IsAny<Guid>()))
            .ThrowsAsync(new InvalidOperationException("boom"));

        // Act
        var act = async () => await handler.Handle(command);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("boom");
    }

    [Test]
    public async Task Handle_WhenRemovingPause_ShouldSaveUpdatedApprenticeship()
    {
        // Arrange
        var learningDomainModel = Fixture.BuildLearning();
        var command = BuildCommand(learningDomainModel);
        command.Request.PauseDate = null;
        var handler = GetUpdateOnProgrammeCommandHandler();

        var episode = learningDomainModel.Episodes.First();
        episode.UpdatePause(GetValidPauseDate(learningDomainModel));

        LearningRepositoryMock
            .Setup(r => r.GetApprenticeshipLearning(learningDomainModel.LearningKey))
            .ReturnsAsync(learningDomainModel);

        // Act
        await handler.Handle(command);

        // Assert
        LearningRepositoryMock.Verify(r => r.Update(It.Is<ApprenticeshipLearning>(a =>
            a == learningDomainModel &&
            a.Episodes.First().PauseDate == null)), Times.Once);
    }

    /// <summary>
    /// Gets a pause date that falls within the valid range for pausing an apprenticeship.
    /// </summary>
    private static DateTime GetValidPauseDate(ApprenticeshipLearning learning)
    {
        var earliestStartDate = learning.Episodes
            .SelectMany(e => e.Prices)
            .Min(e => e.StartDate);

        var latestEndDate = learning.Episodes
            .SelectMany(e => e.Prices)
            .Max(e => e.EndDate);

        var apprenticeshipDurationDays = (latestEndDate - earliestStartDate).TotalDays;

        var validPauseDate = earliestStartDate.AddDays(apprenticeshipDurationDays / 2);
        return validPauseDate;
    }
}

