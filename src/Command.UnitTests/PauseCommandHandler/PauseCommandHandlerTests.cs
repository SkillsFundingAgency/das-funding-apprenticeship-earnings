using AutoFixture;
using FluentAssertions;
using Moq;
using SFA.DAS.Funding.ApprenticeshipEarnings.Command.PauseCommand;
using SFA.DAS.Funding.ApprenticeshipEarnings.DataAccess.Entities;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Apprenticeship;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Repositories;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Services;
using SFA.DAS.Funding.ApprenticeshipEarnings.Types;
using SFA.DAS.Learning.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.Command.UnitTests.PauseCommandHandler;

[TestFixture]
public class PauseCommandHandlerTests
{
    private Fixture _fixture = new Fixture();
    private Mock<IApprenticeshipRepository> _repositoryMock;
    private Mock<ISystemClockService> _systemClockMock;
    private SFA.DAS.Funding.ApprenticeshipEarnings.Command.PauseCommand.PauseCommandHandler _handler;

    private Apprenticeship _apprenticeship;
    private Guid _apprenticeshipKey;
    private DateTime _pauseDate;

    [SetUp]
    public void SetUp()
    {
        _repositoryMock = new Mock<IApprenticeshipRepository>();
        _systemClockMock = new Mock<ISystemClockService>();
        _handler = new SFA.DAS.Funding.ApprenticeshipEarnings.Command.PauseCommand.PauseCommandHandler(_repositoryMock.Object, _systemClockMock.Object);       
        _pauseDate = new DateTime(2024, 10, 10);
        _apprenticeship = _fixture.BuildApprenticeship();
        _apprenticeshipKey = _apprenticeship.ApprenticeshipKey;

        _repositoryMock
            .Setup(r => r.Get(_apprenticeshipKey))
            .ReturnsAsync(_apprenticeship);
    }

    [Test]
    public async Task Handle_WhenCalled_ShouldSaveUpdatedApprenticeship()
    {
        // Arrange
        var command = new PauseCommand.PauseCommand(_apprenticeshipKey, new PauseRequest { PauseDate = _pauseDate });

        // Act
        await _handler.Handle(command);

        // Assert
        _repositoryMock.Verify(r => r.Update(It.Is<Apprenticeship>(a => 
            a == _apprenticeship && 
            a.ApprenticeshipEpisodes.First().PauseDate == _pauseDate)), Times.Once);
    }

    [Test]
    public async Task Handle_ShouldPropagateExceptionsFromRepository()
    {
        // Arrange
        var command = new PauseCommand.PauseCommand(_apprenticeshipKey, new PauseRequest { PauseDate = _pauseDate });


        _repositoryMock
            .Setup(r => r.Get(It.IsAny<Guid>()))
            .ThrowsAsync(new InvalidOperationException("boom"));

        // Act
        var act = async () => await _handler.Handle(command);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("boom");
    }

}

