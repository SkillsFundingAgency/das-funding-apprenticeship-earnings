using AutoFixture;
using Moq;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Apprenticeship;
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
        var apprenticeship = Fixture.BuildApprenticeship();
        var command = BuildCommand(apprenticeship);
        command.Request.WithdrawalDate = new DateTime(2024, 11, 30);
        var handler = GetUpdateOnProgrammeCommandHandler();

        SystemClockServiceMock.Setup(x => x.UtcNow).Returns(new DateTime(2024, 12, 1));
        ApprenticeshipRepositoryMock.Setup(repo => repo.Get(It.IsAny<Guid>())).ReturnsAsync(apprenticeship);

        // Act
        await handler.Handle(command);

        // Assert
        ApprenticeshipRepositoryMock.Verify(x => x.Get(command.ApprenticeshipKey), Times.Once);
        ApprenticeshipRepositoryMock.Verify(x => x.Update(It.IsAny<Apprenticeship>()), Times.Once);
    }
}