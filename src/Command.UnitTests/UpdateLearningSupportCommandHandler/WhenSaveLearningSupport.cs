using AutoFixture;
using Microsoft.Extensions.Logging;
using Moq;
using SFA.DAS.Funding.ApprenticeshipEarnings.Command;
using SFA.DAS.Funding.ApprenticeshipEarnings.Command.UpdateLearningSupportCommand;
using SFA.DAS.Funding.ApprenticeshipEarnings.DataAccess.Entities;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Apprenticeship;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Repositories;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Services;
using SFA.DAS.Funding.ApprenticeshipEarnings.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommandHandler = SFA.DAS.Funding.ApprenticeshipEarnings.Command.UpdateLearningSupportCommand.UpdateLearningSupportCommandHandler;
using SaveCommand = SFA.DAS.Funding.ApprenticeshipEarnings.Command.UpdateLearningSupportCommand.UpdateLearningSupportCommand;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.Command.UnitTests.UpdateLearningSupportCommandHandler;

[TestFixture]
public class WhenSaveLearningSupport
{
    private readonly Fixture _fixture = new();
    private Mock<ILogger<CommandHandler>> _mockLogger;
    private Mock<IApprenticeshipRepository> _mockApprenticeshipRepository;
    private Mock<ISystemClockService> _mockSystemClockService;
    private CommandHandler _handler;

    [SetUp]
    public void SetUp()
    {
        _mockLogger = new Mock<ILogger<CommandHandler>>();
        _mockApprenticeshipRepository = new Mock<IApprenticeshipRepository>();
        _mockSystemClockService = new Mock<ISystemClockService>();

        _handler = new CommandHandler(
            _mockLogger.Object,
            _mockApprenticeshipRepository.Object,
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

        var apprenticeshipModel = _fixture.Create<LearningModel>();
        apprenticeshipModel.Episodes = new List<EpisodeModel>
        {
            _fixture.Create<EpisodeModel>()
        };

        var apprenticeship = Apprenticeship.Get(apprenticeshipModel);
        _mockApprenticeshipRepository
            .Setup(repo => repo.Get(command.ApprenticeshipKey))
            .ReturnsAsync(apprenticeship);

        // Act
        await _handler.Handle(command);

        // Assert
        _mockApprenticeshipRepository.Verify(repo => repo.Update(apprenticeship), Times.Once);
    }
}
