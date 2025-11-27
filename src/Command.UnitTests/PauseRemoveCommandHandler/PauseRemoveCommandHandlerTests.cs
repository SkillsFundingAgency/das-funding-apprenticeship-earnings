using AutoFixture;
using FluentAssertions;
using Moq;
using SFA.DAS.Funding.ApprenticeshipEarnings.Command.PauseRemoveCommand;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Apprenticeship;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Repositories;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.Command.UnitTests.PauseRemoveCommandHandler;

[TestFixture]
public class PauseRemoveCommandHandlerTests
{
    private Fixture _fixture = new Fixture();
    private Mock<IApprenticeshipRepository> _repositoryMock;
    private Mock<ISystemClockService> _systemClockMock;
    private SFA.DAS.Funding.ApprenticeshipEarnings.Command.PauseRemoveCommand.PauseRemoveCommandHandler _handler;

    private Apprenticeship _apprenticeship;
    private Guid _apprenticeshipKey;
    private DateTime _pauseDate;

    [SetUp]
    public void SetUp()
    {
        _repositoryMock = new Mock<IApprenticeshipRepository>();
        _systemClockMock = new Mock<ISystemClockService>();
        _handler = new SFA.DAS.Funding.ApprenticeshipEarnings.Command.PauseRemoveCommand.PauseRemoveCommandHandler(_repositoryMock.Object, _systemClockMock.Object);
        _apprenticeship = _fixture.BuildApprenticeship();
        _pauseDate = GetValidPauseDate(_apprenticeship);
        _apprenticeshipKey = _apprenticeship.ApprenticeshipKey;

        _apprenticeship.Pause(_pauseDate, _systemClockMock.Object);

        _repositoryMock
            .Setup(r => r.Get(_apprenticeshipKey))
            .ReturnsAsync(_apprenticeship);

    }

    [Test]
    public async Task Handle_WhenCalled_ShouldSaveUpdatedApprenticeship()
    {
        // Arrange
        var command = new PauseRemoveCommand.PauseRemoveCommand(_apprenticeshipKey);

        // Act
        await _handler.Handle(command);

        // Assert
        _repositoryMock.Verify(r => r.Update(It.Is<Apprenticeship>(a =>
            a == _apprenticeship &&
            a.ApprenticeshipEpisodes.First().PauseDate == null)), Times.Once);
    }

    [Test]
    public async Task Handle_ShouldPropagateExceptionsFromRepository()
    {
        // Arrange
        var command = new PauseRemoveCommand.PauseRemoveCommand(_apprenticeshipKey);

        _repositoryMock
            .Setup(r => r.Get(It.IsAny<Guid>()))
            .ThrowsAsync(new InvalidOperationException("boom"));

        // Act
        var act = async () => await _handler.Handle(command);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("boom");
    }

    /// <summary>
    /// Gets a pause date that falls within the valid range for pausing an apprenticeship.
    /// </summary>
    private static DateTime GetValidPauseDate(Apprenticeship apprenticeship)
    {
        var earliestStartDate = apprenticeship.ApprenticeshipEpisodes
            .SelectMany(e => e.Prices)
            .Min(e => e.StartDate);

        var latestEndDate = apprenticeship.ApprenticeshipEpisodes
            .SelectMany(e => e.Prices)
            .Max(e => e.EndDate);

        var apprenticeshipDurationDays = (latestEndDate - earliestStartDate).TotalDays;

        var validPauseDate = earliestStartDate.AddDays(apprenticeshipDurationDays / 2);
        return validPauseDate;
    }
}
