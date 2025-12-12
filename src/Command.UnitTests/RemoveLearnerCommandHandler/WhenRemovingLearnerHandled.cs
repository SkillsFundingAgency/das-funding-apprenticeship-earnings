using AutoFixture;
using Moq;
using NServiceBus;
using SFA.DAS.Learning.Types;
using SFA.DAS.Funding.ApprenticeshipEarnings.DataAccess.Entities;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Apprenticeship;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Repositories;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Services;
using SFA.DAS.Funding.ApprenticeshipEarnings.Types;
using SFA.DAS.Funding.ApprenticeshipEarnings.Command.RemoveLearnerCommand;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.Command.UnitTests.ProcessWithdrawnApprenticeshipsCommandHandler;

[TestFixture]
public class WhenRemovingLearnerHandled
{
    private readonly Fixture _fixture = new();
    private readonly Mock<ISystemClockService> _mockSystemClock = new();
    private readonly Mock<IApprenticeshipRepository> _mockRepository = new();

    private void SetupMocks()
    {
        _mockRepository.Reset();
        _mockSystemClock.Setup(x => x.UtcNow).Returns(new DateTime(2024, 12, 1));
    }

    [Test]
    public async Task ThenTheApprenticeshipIsWithdrawnToStart()
    {
        // Arrange
        var apprenticeship = _fixture.BuildApprenticeship();
        var command = new RemoveLearnerCommand.RemoveLearnerCommand(apprenticeship.ApprenticeshipKey);
        var handler = new RemoveLearnerCommandHandler(_mockRepository.Object, _mockSystemClock.Object);

        _mockSystemClock.Setup(x => x.UtcNow).Returns(new DateTime(2024, 12, 1));
        _mockRepository.Setup(repo => repo.Get(It.IsAny<Guid>())).ReturnsAsync(apprenticeship);

        // Act
        await handler.Handle(command);

        // Assert
        _mockRepository.Verify(x => x.Get(command.ApprenticeshipKey), Times.Once);
        _mockRepository.Verify(x => x.Update(It.IsAny<Apprenticeship>()), Times.Once);
    }


}