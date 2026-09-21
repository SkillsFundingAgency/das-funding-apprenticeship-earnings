using FluentAssertions;
using Moq;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Models.Apprenticeship;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.Command.UnitTests.UpdateOnProgrammeCommandHandler;

[TestFixture]
public class WhenLearningNotFound : BaseUpdateCommandHandlerTests
{
    [Test]
    public async Task ThenNoExceptionIsThrownWhenNoEarningsHaveBeenGeneratedForTheLearner()
    {
        // Arrange
        var learningDomainModel = Fixture.BuildLearning();
        var command = BuildCommand(learningDomainModel);
        var handler = GetUpdateOnProgrammeCommandHandler();

        LearningRepositoryMock
            .Setup(r => r.GetApprenticeshipLearning(learningDomainModel.LearningKey))
            .ReturnsAsync((ApprenticeshipLearning)null!);

        // Act
        var act = async () => await handler.Handle(command);

        // Assert
        await act.Should().NotThrowAsync();
        LearningRepositoryMock.Verify(r => r.Update(It.IsAny<ApprenticeshipLearning>()), Times.Never);
    }
}
