using AutoFixture;
using Microsoft.Extensions.Logging;
using Moq;
using SFA.DAS.Funding.ApprenticeshipEarnings.Command.UpdateLearningSupportCommand;
using SFA.DAS.Funding.ApprenticeshipEarnings.DataAccess.Entities;
using SFA.DAS.Funding.ApprenticeshipEarnings.DataAccess.Entities.Apprenticeship;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Models;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Repositories;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Services;
using SFA.DAS.Funding.ApprenticeshipEarnings.Types;
using CommandHandler = SFA.DAS.Funding.ApprenticeshipEarnings.Command.UpdateLearningSupportCommand.UpdateLearningSupportCommandHandler;
using SaveCommand = SFA.DAS.Funding.ApprenticeshipEarnings.Command.UpdateLearningSupportCommand.UpdateLearningSupportCommand;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.Command.UnitTests.UpdateLearningSupportCommandHandler;

[TestFixture]
public class WhenSaveLearningSupport
{
    private readonly Fixture _fixture = new();
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
    public async Task Handle_ShouldCallRepositoryUpdate_WhenLearningSupportPaymentsAreAdded()
    {
        // Arrange
        var command = new SaveCommand(
            _fixture.Create<Guid>(),
            new UpdateLearningSupportRequest { LearningSupport =[ new LearningSupportItem { StartDate = DateTime.Now.AddMonths(-6), EndDate = DateTime.Now} ]}
            );

        var learningEntity = _fixture.Create<LearningEntity>();
        learningEntity.ApprenticeshipEpisodes = new List<ApprenticeshipEpisodeEntity>
        {
            _fixture.Create<ApprenticeshipEpisodeEntity>()
        };

        var learning = Domain.Models.Learning.Get(learningEntity);
        _mockRepository
            .Setup(repo => repo.Get(command.LearningKey))
            .ReturnsAsync(learning);

        // Act
        await _handler.Handle(command);

        // Assert
        _mockRepository.Verify(repo => repo.Update(learning), Times.Once);
    }
}
