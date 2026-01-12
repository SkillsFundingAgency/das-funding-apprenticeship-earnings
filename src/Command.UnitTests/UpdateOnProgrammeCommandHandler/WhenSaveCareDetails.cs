using Moq;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Apprenticeship;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.Command.UnitTests.UpdateOnProgrammeCommandHandler;

[TestFixture]
public class WhenSaveCareDetails : BaseUpdateCommandHandlerTests
{
    [Test]
    public async Task Handle_ShouldUpdateCareDetails_WhenCalled()
    {
        // Arrange
        var apprenticeship = Fixture.BuildApprenticeship();
        var command = BuildCommand(apprenticeship);

        command.Request.Care.HasEHCP = !apprenticeship.HasEHCP;
        command.Request.Care.IsCareLeaver = !apprenticeship.IsCareLeaver;
        command.Request.Care.CareLeaverEmployerConsentGiven =
            !apprenticeship.CareLeaverEmployerConsentGiven;

        var handler = GetUpdateOnProgrammeCommandHandler();

        ApprenticeshipRepositoryMock
            .Setup(repo => repo.Get(It.IsAny<Guid>()))
            .ReturnsAsync(apprenticeship);

        // Act
        await handler.Handle(command);

        // Assert
        ApprenticeshipRepositoryMock.Verify(repo =>
                repo.Update(It.Is<Apprenticeship>(a =>
                    a.HasEHCP == command.Request.Care.HasEHCP &&
                    a.IsCareLeaver == command.Request.Care.IsCareLeaver &&
                    a.CareLeaverEmployerConsentGiven ==
                    command.Request.Care.CareLeaverEmployerConsentGiven
                )),
            Times.Once);
    }
}