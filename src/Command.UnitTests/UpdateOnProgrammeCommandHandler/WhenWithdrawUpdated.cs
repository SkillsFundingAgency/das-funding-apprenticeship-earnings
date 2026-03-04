using AutoFixture;
using Moq;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Models;
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
        LearningRepositoryMock.Setup(repo => repo.Get(It.IsAny<Guid>())).ReturnsAsync(learningDomainModel);

        // Act
        await handler.Handle(command);

        // Assert
        LearningRepositoryMock.Verify(x => x.Get(command.ApprenticeshipKey), Times.Once);
        LearningRepositoryMock.Verify(x => x.Update(It.IsAny<Domain.Models.Learning>()), Times.Once);
    }
}