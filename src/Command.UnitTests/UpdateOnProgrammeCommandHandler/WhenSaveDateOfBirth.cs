using AutoFixture;
using Microsoft.Extensions.Logging;
using Moq;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Apprenticeship;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Repositories;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Services;
using SFA.DAS.Funding.ApprenticeshipEarnings.Types;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.Command.UnitTests.UpdateOnProgrammeCommandHandler;

[TestFixture]
public class WhenSaveDateOfBirth : BaseUpdateCommandHandlerTests
{

    [Test]
    public async Task Handle_ShouldUpdateApprenticeship_WhenCalled()
    {
        // Arrange
        var apprenticeship = Fixture.BuildApprenticeship();
        var command = BuildCommand(apprenticeship);
        command.Request.DateOfBirth = apprenticeship.DateOfBirth.AddYears(-1);
        var handler = GetUpdateOnProgrammeCommandHandler();

        ApprenticeshipRepositoryMock.Setup(repo => repo.Get(It.IsAny<Guid>())).ReturnsAsync(apprenticeship);

        // Act
        await handler.Handle(command);

        // Assert
        ApprenticeshipRepositoryMock.Verify(repo => repo.Update(It.Is<Apprenticeship>(a =>
            a.DateOfBirth == command.Request.DateOfBirth
        )), Times.Once);
    }
}