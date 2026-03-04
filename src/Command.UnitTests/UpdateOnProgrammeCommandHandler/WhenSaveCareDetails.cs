using Moq;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Models;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.Command.UnitTests.UpdateOnProgrammeCommandHandler;

[TestFixture]
public class WhenSaveCareDetails : BaseUpdateCommandHandlerTests
{
    [Test]
    public async Task Handle_ShouldUpdateCareDetails_WhenCalled()
    {
        // Arrange
        var learningDomainModel = Fixture.BuildLearning();
        var command = BuildCommand(learningDomainModel);

        command.Request.Care.HasEHCP = !learningDomainModel.HasEHCP;
        command.Request.Care.IsCareLeaver = !learningDomainModel.IsCareLeaver;
        command.Request.Care.CareLeaverEmployerConsentGiven =
            !learningDomainModel.CareLeaverEmployerConsentGiven;

        var handler = GetUpdateOnProgrammeCommandHandler();

        LearningRepositoryMock
            .Setup(repo => repo.Get(It.IsAny<Guid>()))
            .ReturnsAsync(learningDomainModel);

        // Act
        await handler.Handle(command);

        // Assert
        LearningRepositoryMock.Verify(repo =>
                repo.Update(It.Is<Domain.Models.Learning>(a =>
                    a.HasEHCP == command.Request.Care.HasEHCP &&
                    a.IsCareLeaver == command.Request.Care.IsCareLeaver &&
                    a.CareLeaverEmployerConsentGiven ==
                    command.Request.Care.CareLeaverEmployerConsentGiven
                )),
            Times.Once);
    }
}