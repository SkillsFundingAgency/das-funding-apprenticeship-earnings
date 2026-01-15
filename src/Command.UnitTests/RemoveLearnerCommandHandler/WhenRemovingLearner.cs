using AutoFixture;
using FluentAssertions;
using Moq;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Apprenticeship;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Repositories;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Services;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.Command.UnitTests.RemoveLearnerCommandHandler;

[TestFixture]
public class WhenRemovingLearner
{
    private Fixture _fixture = null!;
    private Mock<ISystemClockService> _mockSystemClock = null!;
    private Mock<IApprenticeshipRepository> _mockRepository = null!;

    [SetUp]
    public void SetUp()
    {
        _fixture = new Fixture();
        _mockSystemClock = new Mock<ISystemClockService>();
        _mockRepository = new Mock<IApprenticeshipRepository>();
    }

    [Test]
    public async Task ThenTheApprenticeshipIsWithdrawnToStart()
    {
        // Arrange
        var apprenticeship = _fixture.BuildApprenticeship();
        var command = new RemoveLearnerCommand.RemoveLearnerCommand(apprenticeship.ApprenticeshipKey);
        var handler = new RemoveLearnerCommand.RemoveLearnerCommandHandler(_mockRepository.Object, _mockSystemClock.Object);

        _mockSystemClock.Setup(x => x.UtcNow).Returns(new DateTime(2024, 12, 1));
        _mockRepository.Setup(repo => repo.Get(It.IsAny<Guid>())).ReturnsAsync(apprenticeship);

        // Act
        await handler.Handle(command);

        // Assert
        _mockRepository.Verify(x => x.Get(command.ApprenticeshipKey), Times.Once);
        _mockRepository.Verify(x => x.Update(It.Is<Apprenticeship>(a => a.ApprenticeshipKey == apprenticeship.ApprenticeshipKey)), Times.Once);
    }

    [Test]
    public async Task ThenEnglishAndMathsIsRemoved()
    {
        // Arrange
        var apprenticeship = _fixture.BuildApprenticeship();
        apprenticeship.UpdateMathsAndEnglishCourses(_fixture.Create<List<MathsAndEnglish>>(), _mockSystemClock.Object);
        
        var command = new RemoveLearnerCommand.RemoveLearnerCommand(apprenticeship.ApprenticeshipKey);
        var handler = new RemoveLearnerCommand.RemoveLearnerCommandHandler(_mockRepository.Object, _mockSystemClock.Object);

        _mockSystemClock.Setup(x => x.UtcNow).Returns(new DateTime(2024, 12, 1));
        _mockRepository.Setup(repo => repo.Get(It.IsAny<Guid>())).ReturnsAsync(apprenticeship);

        Apprenticeship updated = null!;

        _mockRepository
            .Setup(x => x.Update(It.IsAny<Apprenticeship>()))
            .Callback<Apprenticeship>(a => updated = a);

        // Act
        await handler.Handle(command);

        // Assert
        _mockRepository.Verify(x => x.Update(It.IsAny<Apprenticeship>()), Times.Once);
        updated.GetCurrentEpisode(_mockSystemClock.Object).EarningsProfile.MathsAndEnglishCourses.Should().BeEmpty();
    }

    [Test]
    public async Task ThenAdditionalEarningsAreRemoved()
    {
        // Arrange
        var additionalPayment = new AdditionalPayment(2425, 1, 1000, DateTime.UtcNow, "LearningSupport");

        var apprenticeship = _fixture.BuildApprenticeship();
        apprenticeship.AddAdditionalEarnings([additionalPayment], "LearningSupport", _mockSystemClock.Object);

        var command = new RemoveLearnerCommand.RemoveLearnerCommand(apprenticeship.ApprenticeshipKey);
        var handler = new RemoveLearnerCommand.RemoveLearnerCommandHandler(_mockRepository.Object, _mockSystemClock.Object);

        _mockSystemClock.Setup(x => x.UtcNow).Returns(new DateTime(2024, 12, 1));
        _mockRepository.Setup(repo => repo.Get(It.IsAny<Guid>())).ReturnsAsync(apprenticeship);

        Apprenticeship updated = null!;

        _mockRepository
            .Setup(x => x.Update(It.IsAny<Apprenticeship>()))
            .Callback<Apprenticeship>(a => updated = a);

        // Act
        await handler.Handle(command);

        // Assert
        _mockRepository.Verify(x => x.Update(It.IsAny<Apprenticeship>()), Times.Once);
        updated.GetCurrentEpisode(_mockSystemClock.Object)
            .EarningsProfile.AdditionalPayments.Should().BeEmpty();
    }

    [Test]
    public async Task ThenPeriodsInLearningAreRemoved()
    {
        // Arrange
        var apprenticeship = _fixture.BuildApprenticeship();

        var command = new RemoveLearnerCommand.RemoveLearnerCommand(apprenticeship.ApprenticeshipKey);
        var handler = new RemoveLearnerCommand.RemoveLearnerCommandHandler(_mockRepository.Object, _mockSystemClock.Object);

        _mockSystemClock.Setup(x => x.UtcNow).Returns(new DateTime(2024, 12, 1));
        _mockRepository.Setup(repo => repo.Get(It.IsAny<Guid>())).ReturnsAsync(apprenticeship);

        Apprenticeship updated = null!;

        _mockRepository
            .Setup(x => x.Update(It.IsAny<Apprenticeship>()))
            .Callback<Apprenticeship>(a => updated = a);

        // Act
        await handler.Handle(command);

        // Assert
        _mockRepository.Verify(x => x.Update(It.IsAny<Apprenticeship>()), Times.Once);

        updated.GetCurrentEpisode(_mockSystemClock.Object).EpisodePeriodsInLearning.Should().BeEmpty();
    }
}