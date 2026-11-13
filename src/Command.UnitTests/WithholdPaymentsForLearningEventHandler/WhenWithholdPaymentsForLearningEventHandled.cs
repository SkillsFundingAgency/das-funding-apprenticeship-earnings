using AutoFixture;
using Microsoft.Extensions.Logging;
using Moq;
using SFA.DAS.Funding.ApprenticeshipEarnings.Infrastructure.Configuration;
using SFA.DAS.Funding.ApprenticeshipEarnings.Types;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.Command.UnitTests.WithholdPaymentsForLearningEventHandler;

[TestFixture]
public class WhenWithholdPaymentsForLearningEventHandled
{
    private const string PaymentsEndpoint = "payments-queue-name";

    private Mock<IMessageHandlerContext> _mockContext = null!;
    private MessageHandlers.Handlers.WithholdPaymentsForLearningEventHandler _handler = null!;
    private Fixture _fixture = null!;

    [SetUp]
    public void SetUp()
    {
        _mockContext = new Mock<IMessageHandlerContext>();
        _fixture = new Fixture();

        _handler = new MessageHandlers.Handlers.WithholdPaymentsForLearningEventHandler(
            new PaymentsConfiguration { PaymentsEndpoint = PaymentsEndpoint },
            Mock.Of<ILogger<MessageHandlers.Handlers.WithholdPaymentsForLearningEventHandler>>());
    }

    [Test]
    public async Task ThenTheEventIsSentToPayments()
    {
        // Arrange
        var message = _fixture.Create<WithholdPaymentsForLearningEvent>();

        // Act
        await _handler.Handle(message, _mockContext.Object);

        // Assert
        _mockContext.Verify(x => x.Send(
            message,
            It.Is<SendOptions>(o => o.GetDestination() == PaymentsEndpoint)), Times.Once);
    }

    [Test]
    public async Task ThenTheEventIsNotPublishedAgain()
    {
        // Arrange
        var message = _fixture.Create<WithholdPaymentsForLearningEvent>();

        // Act
        await _handler.Handle(message, _mockContext.Object);

        // Assert
        _mockContext.Verify(x => x.Publish(It.IsAny<object>(), It.IsAny<PublishOptions>()), Times.Never);
    }
}
