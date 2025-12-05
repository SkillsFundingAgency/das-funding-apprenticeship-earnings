using AutoFixture;
using Microsoft.Extensions.Logging;
using Moq;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Apprenticeship;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Repositories;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Services;
using SFA.DAS.Funding.ApprenticeshipEarnings.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.Command.UnitTests.SaveDateOfBirthCommandHandler;

[TestFixture]
public class WhenSaveDateOfBirth
{
    private readonly Fixture _fixture = new();
    private Mock<IApprenticeshipRepository> _apprenticeshipRepositoryMock;
    private Mock<ISystemClockService> _systemClockServiceMock;
    private SaveDateOfBirthCommand.SaveDateOfBirthCommandHandler _handler;

    [SetUp]
    public void SetUp()
    {
        _apprenticeshipRepositoryMock = new Mock<IApprenticeshipRepository>();
        _systemClockServiceMock = new Mock<ISystemClockService>();
        _systemClockServiceMock.Setup(x => x.UtcNow).Returns(DateTime.UtcNow);

        _handler = new SaveDateOfBirthCommand.SaveDateOfBirthCommandHandler(
            _apprenticeshipRepositoryMock.Object,
            _systemClockServiceMock.Object);
    }

    [Test]
    public async Task Handle_ShouldUpdateApprenticeship_WhenCalled()
    {
        // Arrange
        var command = _fixture.Create<SaveDateOfBirthCommand.SaveDateOfBirthCommand>();
        var apprenticeship = _fixture.BuildApprenticeship();
        _apprenticeshipRepositoryMock.Setup(repo => repo.Get(It.IsAny<Guid>())).ReturnsAsync(apprenticeship);

        // Act
        await _handler.Handle(command);

        // Assert
        _apprenticeshipRepositoryMock.Verify(repo => repo.Update(It.Is<Apprenticeship>(a =>
            a.DateOfBirth == command.DateOfBirth
        )), Times.Once);
    }
}