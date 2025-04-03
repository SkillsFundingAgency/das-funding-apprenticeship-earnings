using AutoFixture;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using SFA.DAS.Funding.ApprenticeshipEarnings.Command.SaveCareDetailsCommand;
using SFA.DAS.Funding.ApprenticeshipEarnings.DataAccess.Entities;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Apprenticeship;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.Command.UnitTests.SaveCareDetailsCommandHandler;

[TestFixture]
public class WhenHandleSaveCareDetails
{
    private readonly Fixture _fixture = new();
    private Mock<ILogger<SaveCareDetailsCommand.SaveCareDetailsCommandHandler>> _loggerMock;
    private Mock<IApprenticeshipRepository> _apprenticeshipRepositoryMock;
    private SaveCareDetailsCommand.SaveCareDetailsCommandHandler _handler;

    [SetUp]
    public void SetUp()
    {
        _loggerMock = new Mock<ILogger<SaveCareDetailsCommand.SaveCareDetailsCommandHandler>>();
        _apprenticeshipRepositoryMock = new Mock<IApprenticeshipRepository>();
        _handler = new SaveCareDetailsCommand.SaveCareDetailsCommandHandler(_loggerMock.Object, _apprenticeshipRepositoryMock.Object);
    }

    [Test]
    public async Task Handle_ShouldUpdateApprenticeship_WhenCalled()
    {
        // Arrange
        var command = _fixture.Create<SaveCareDetailsCommand.SaveCareDetailsCommand>();
        var apprenticeshipModel = _fixture.Create<ApprenticeshipModel>();
        var apprenticeship = Apprenticeship.Get(apprenticeshipModel);
        _apprenticeshipRepositoryMock.Setup(repo => repo.Get(It.IsAny<Guid>())).ReturnsAsync(apprenticeship);

        // Act
        await _handler.Handle(command);

        // Assert
        _apprenticeshipRepositoryMock.Verify(repo => repo.Update(It.Is<Apprenticeship>(a =>
            a.HasEHCP == command.HasEHCP &&
            a.IsCareLeaver == command.IsCareLeaver &&
            a.CareLeaverEmployerConsentGiven == command.CareLeaverEmployerConsentGiven
        )), Times.Once);
    }

    [Test]
    public void Handle_ShouldLogError_WhenExceptionThrown()
    {
        // Arrange
        var command = _fixture.Create<SaveCareDetailsCommand.SaveCareDetailsCommand>(); 
        _apprenticeshipRepositoryMock.Setup(repo => repo.Get(It.IsAny<Guid>())).ThrowsAsync(new Exception("Test exception"));

        // Act & Assert
        Assert.ThrowsAsync<Exception>(() => _handler.Handle(command));
    }
}
