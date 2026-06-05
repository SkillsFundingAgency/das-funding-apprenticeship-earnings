using AutoFixture;
using Microsoft.Extensions.Logging;
using Moq;
using SFA.DAS.Funding.ApprenticeshipEarnings.Types;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.Command.UnitTests.ShortCoursePayableEarningsUpdatedEventHandler;

[TestFixture]
public class WhenShortCoursePayableEarningsUpdated
{
    private Mock<ICommandHandler<SendShortCoursePayableEarningsToPaymentsCommand.SendShortCoursePayableEarningsToPaymentsCommand>> _mockCommandHandler = null!;
    private Mock<IMessageHandlerContext> _mockContext = null!;
    private Fixture _fixture = null!;

    [SetUp]
    public void SetUp()
    {
        _mockCommandHandler = new Mock<ICommandHandler<SendShortCoursePayableEarningsToPaymentsCommand.SendShortCoursePayableEarningsToPaymentsCommand>>();
        _mockContext = new Mock<IMessageHandlerContext>();
        _fixture = new Fixture();
    }

    [Test]
    public async Task ThenSendShortCoursePayableEarningsToPaymentsCommandIsInvokedWithCorrectKeys()
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
            It.Is<SendShortCoursePayableEarningsToPaymentsCommand.SendShortCoursePayableEarningsToPaymentsCommand>(c =>
                c.ShortCoursePayableEarningsUpdatedEvent.LearningKey == message.LearningKey &&
                c.ShortCoursePayableEarningsUpdatedEvent.EpisodeKey == message.EpisodeKey),
            It.IsAny<CancellationToken>()), Times.Once);
    }
}
