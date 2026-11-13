using AutoFixture;
using Moq;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Models.Apprenticeship;
using SFA.DAS.Funding.ApprenticeshipEarnings.Types;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.Command.UnitTests.UpdateOnProgrammeCommandHandler;

[TestFixture]
public class WhenFurtherApprovalUpdated : BaseUpdateCommandHandlerTests
{
    [Test]
    public async Task Handle_WhenFurtherApprovalRequired_ShouldSaveEpisodeAsNeedingFurtherApproval()
    {
        // Arrange
        var learningDomainModel = BuildLearning(furtherApprovalNeeded: false);
        var command = BuildCommand(learningDomainModel);
        command.Request.FurtherApprovalRequired = true;

        var handler = GetUpdateOnProgrammeCommandHandler();

        LearningRepositoryMock
            .Setup(r => r.GetApprenticeshipLearning(learningDomainModel.LearningKey))
            .ReturnsAsync(learningDomainModel);

        // Act
        await handler.Handle(command);

        // Assert
        LearningRepositoryMock.Verify(r => r.Update(It.Is<ApprenticeshipLearning>(a =>
            a == learningDomainModel &&
            a.Episodes.First().FurtherApprovalNeeded)), Times.Once);
    }

    [Test]
    public async Task Handle_WhenFurtherApprovalNoLongerRequired_ShouldSaveEpisodeAsNotNeedingFurtherApproval()
    {
        // Arrange
        var learningDomainModel = BuildLearning(furtherApprovalNeeded: true);
        var command = BuildCommand(learningDomainModel);
        command.Request.FurtherApprovalRequired = false;

        var handler = GetUpdateOnProgrammeCommandHandler();

        LearningRepositoryMock
            .Setup(r => r.GetApprenticeshipLearning(learningDomainModel.LearningKey))
            .ReturnsAsync(learningDomainModel);

        // Act
        await handler.Handle(command);

        // Assert
        LearningRepositoryMock.Verify(r => r.Update(It.Is<ApprenticeshipLearning>(a =>
            a == learningDomainModel &&
            !a.Episodes.First().FurtherApprovalNeeded)), Times.Once);
    }

    /// <summary>
    /// SLD platform, so setting the flag never raises the withhold event (which would need a numeric ULN);
    /// the event itself is covered by the domain tests.
    /// </summary>
    private ApprenticeshipLearning BuildLearning(bool furtherApprovalNeeded)
    {
        var entity = Fixture.BuildLearningModel();
        var episode = entity.Episodes.Single();
        episode.FundingPlatform = FundingPlatform.SLD;
        episode.FurtherApprovalNeeded = furtherApprovalNeeded;
        return ApprenticeshipLearning.Get(entity);
    }
}
