using Microsoft.Extensions.Logging;
using Moq;
using SFA.DAS.Funding.ApprenticeshipEarnings.Command.UpdateLearningSupportCommand;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Models.Apprenticeship;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Repositories;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Services;
using SFA.DAS.Funding.ApprenticeshipEarnings.Types;
using CommandHandler = SFA.DAS.Funding.ApprenticeshipEarnings.Command.UpdateLearningSupportCommand.UpdateLearningSupportCommandHandler;
using SaveCommand = SFA.DAS.Funding.ApprenticeshipEarnings.Command.UpdateLearningSupportCommand.UpdateLearningSupportCommand;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.Command.UnitTests.UpdateLearningSupportCommandHandler;

[TestFixture]
public class WhenLearningDomainModelNotFound
{
    private Mock<ILogger<CommandHandler>> _mockLogger;
    private Mock<ILearningRepository> _mockRepository;
    private Mock<ISystemClockService> _mockSystemClockService;
    private CommandHandler _handler;

    [SetUp]
    public void SetUp()
    {
        _mockLogger = new Mock<ILogger<CommandHandler>>();
        _mockRepository = new Mock<ILearningRepository>();
        _mockSystemClockService = new Mock<ISystemClockService>();

        _handler = new CommandHandler(
            _mockLogger.Object,
            _mockRepository.Object,
            _mockSystemClockService.Object);
    }

    [Test]
    public async Task Handle_ShouldReturnGracefully_WhenLearningDomainModelIsNull()
    {
        // Arrange
        var command = new SaveCommand(
            Guid.NewGuid(),
            new UpdateLearningSupportRequest { LearningSupport = [new LearningSupportItem { StartDate = DateTime.Now.AddMonths(-6), EndDate = DateTime.Now }] });

        _mockRepository
            .Setup(repo => repo.GetApprenticeshipLearning(command.LearningKey))
            .ReturnsAsync((ApprenticeshipLearning)null);

        // Act
        Assert.DoesNotThrowAsync(async () => await _handler.Handle(command));

        // Assert
        _mockRepository.Verify(repo => repo.Update(It.IsAny<ApprenticeshipLearning>()), Times.Never);
    }
}
