using AutoFixture;
using Microsoft.Extensions.Logging;
using Moq;
using NServiceBus;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Repositories;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Services;
using SFA.DAS.Funding.ApprenticeshipEarnings.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Apprenticeship = SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Apprenticeship.Apprenticeship;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.Command.UnitTests.ReReleaseEarningsGeneratedCommandHandler;

[TestFixture]
public class WhenHandleReReleaseEarningsGeneratedEvent
{
    private Fixture _fixture;
    private Mock<ILogger<ReReleaseEarningsGeneratedCommand.ReReleaseEarningsGeneratedCommandHandler>> _mockLogger;
    private Mock<IEarningsQueryRepository> _mockEarningsQueryRepository;
    private Mock<IEarningsGeneratedEventBuilder> _mockEarningsGeneratedEventBuilder;
    private Mock<ISystemClockService> _mockSystemClock;
    private Mock<IMessageSession> _mockMessageSession;
    private ReReleaseEarningsGeneratedCommand.ReReleaseEarningsGeneratedCommandHandler _handler;

    [SetUp]
    public void SetUp()
    {
        _fixture = new Fixture();
        _mockLogger = new Mock<ILogger<ReReleaseEarningsGeneratedCommand.ReReleaseEarningsGeneratedCommandHandler>>();
        _mockEarningsQueryRepository = new Mock<IEarningsQueryRepository>();
        _mockEarningsGeneratedEventBuilder = new Mock<IEarningsGeneratedEventBuilder>();
        _mockSystemClock = new Mock<ISystemClockService>();
        _mockMessageSession = new Mock<IMessageSession>();

        _handler = new ReReleaseEarningsGeneratedCommand.ReReleaseEarningsGeneratedCommandHandler(
            _mockLogger.Object,
            _mockEarningsQueryRepository.Object,
            _mockEarningsGeneratedEventBuilder.Object,
            _mockSystemClock.Object,
            _mockMessageSession.Object);
    }

    [Test]
    public async Task Handle_ShouldNotPublishWhenNoApprenticeshipFound()
    {
        // Arrange
        var command = new ReReleaseEarningsGeneratedCommand.ReReleaseEarningsGeneratedCommand(12345);
        _mockEarningsQueryRepository.Setup(x => x.GetApprenticeships(command.Ukprn, It.IsAny<DateTime>(),It.IsAny<bool>())).Returns((List<Apprenticeship>)null);

        // Act
        await _handler.Handle(command);

        // Assert
        _mockMessageSession.Verify(x => x.Publish(It.IsAny<EarningsGeneratedEvent>(), It.IsAny<PublishOptions>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Test]
    public async Task Handle_ShouldPublishEvent_ForEachApprenticeship()
    {
        // Arrange
        var command = new ReReleaseEarningsGeneratedCommand.ReReleaseEarningsGeneratedCommand(12345);
        var apprenticeships = _fixture.Create<List<Apprenticeship>>();
        var eventMessage = new EarningsGeneratedEvent();
        _mockEarningsQueryRepository.Setup(x => x.GetApprenticeships(command.Ukprn, It.IsAny<DateTime>(), It.IsAny<bool>())).Returns(apprenticeships);
        _mockEarningsGeneratedEventBuilder.Setup(x => x.ReGenerate(It.IsAny<Apprenticeship>())).Returns(eventMessage);

        // Act
        await _handler.Handle(command);

        // Assert
        _mockMessageSession.Verify(x => x.Publish(eventMessage, It.IsAny<PublishOptions>(), It.IsAny<CancellationToken>()), Times.Exactly(apprenticeships.Count));
    }
}
