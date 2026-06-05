using AutoFixture;
using Microsoft.Extensions.Logging;
using Moq;
using NServiceBus;
using SFA.DAS.Funding.ApprenticeshipEarnings.Types;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.Command.UnitTests.ShortCoursePayableEarningsUpdatedEventHandlerTests;

[TestFixture]
public class WhenShortCoursePayableEarningsUpdated
{
    private Mock<global::SFA.DAS.Funding.ApprenticeshipEarnings.Command.ICommandHandler<global::SFA.DAS.Funding.ApprenticeshipEarnings.Command.ProcessShortCoursePayableEarningsUpdatedCommand.ProcessShortCoursePayableEarningsUpdatedCommand>> _mockCommandHandler = null!;
    private Mock<IMessageHandlerContext> _mockContext = null!;
    private Fixture _fixture = null!;

    [SetUp]
    public void SetUp()
    {
        _mockCommandHandler = new Mock<global::SFA.DAS.Funding.ApprenticeshipEarnings.Command.ICommandHandler<global::SFA.DAS.Funding.ApprenticeshipEarnings.Command.ProcessShortCoursePayableEarningsUpdatedCommand.ProcessShortCoursePayableEarningsUpdatedCommand>>();
        _mockContext = new Mock<IMessageHandlerContext>();
        _fixture = new Fixture();
    }

    [Test]
    public async Task ThenProcessShortCoursePayableEarningsUpdatedCommandIsInvokedWithCorrectKeys()
    {
        // Arrange
        var message = _fixture.Create<ShortCoursePayableEarningsUpdatedEvent>();

        var handler = new MessageHandlers.Handlers.ShortCoursePayableEarningsUpdatedEventHandler(
            _mockCommandHandler.Object,
            Mock.Of<ILogger<MessageHandlers.Handlers.ShortCoursePayableEarningsUpdatedEventHandler>>());

        // Act
        await handler.Handle(message, _mockContext.Object);

        // Assert
        _mockCommandHandler.Verify(x => x.Handle(
            It.Is<global::SFA.DAS.Funding.ApprenticeshipEarnings.Command.ProcessShortCoursePayableEarningsUpdatedCommand.ProcessShortCoursePayableEarningsUpdatedCommand>(c =>
                c.ShortCoursePayableEarningsUpdatedEvent.LearningKey == message.LearningKey &&
                c.ShortCoursePayableEarningsUpdatedEvent.EpisodeKey == message.EpisodeKey),
            It.IsAny<CancellationToken>()), Times.Once);
    }
}
