using AutoFixture;
using Microsoft.Extensions.Logging;
using Moq;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Models;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Models.Apprenticeship;
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
        var learningDomainModel = Fixture.BuildLearning();
        var command = BuildCommand(learningDomainModel);
        command.Request.DateOfBirth = learningDomainModel.DateOfBirth.AddYears(-1);
        var handler = GetUpdateOnProgrammeCommandHandler();

        LearningRepositoryMock.Setup(repo => repo.GetApprenticeshipLearning(It.IsAny<Guid>())).ReturnsAsync(learningDomainModel);

        // Act
        await handler.Handle(command);

        // Assert
        LearningRepositoryMock.Verify(repo => repo.Update(It.Is<ApprenticeshipLearning>(a =>
            a.DateOfBirth == command.Request.DateOfBirth
        )), Times.Once);
    }
}