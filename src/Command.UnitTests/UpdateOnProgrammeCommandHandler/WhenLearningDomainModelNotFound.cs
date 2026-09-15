using Moq;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Models.Apprenticeship;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Repositories;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.Command.UnitTests.UpdateOnProgrammeCommandHandler;

[TestFixture]
public class WhenLearningDomainModelNotFound : BaseUpdateCommandHandlerTests
{
    [Test]
    public async Task Handle_ShouldReturnGracefully_WhenLearningDomainModelIsNull()
    {
        // Arrange
        var learningDomainModel = Fixture.BuildLearning();
        var command = BuildCommand(learningDomainModel);
        var handler = GetUpdateOnProgrammeCommandHandler();

        LearningRepositoryMock.Setup(repo => repo.GetApprenticeshipLearning(It.IsAny<Guid>())).ReturnsAsync((ApprenticeshipLearning)null);

        // Act
        Assert.DoesNotThrowAsync(async () => await handler.Handle(command));

        // Assert
        LearningRepositoryMock.Verify(repo => repo.Update(It.IsAny<ApprenticeshipLearning>()), Times.Never);
    }
}
