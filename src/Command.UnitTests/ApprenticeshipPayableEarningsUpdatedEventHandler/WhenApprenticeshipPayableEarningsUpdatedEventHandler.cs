using Microsoft.Extensions.Logging;
using Moq;
using SFA.DAS.Funding.ApprenticeshipEarnings.Types;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.Command.UnitTests.ApprenticeshipPayableEarningsUpdatedEventHandler;

[TestFixture]
public class WhenApprenticeshipPayableEarningsUpdatedEventHandler
{
    private Mock<ICommandHandler<ReleaseEarningsCommand.ReleaseEarningsCommand>> _mockCommandHandler = null!;
    private Mock<IMessageHandlerContext> _mockContext = null!;

    [SetUp]
    public void SetUp()
    {
        _mockCommandHandler = new Mock<ICommandHandler<ReleaseEarningsCommand.ReleaseEarningsCommand>>();
        _mockContext = new Mock<IMessageHandlerContext>();
    }

    [Test]
    public async Task ThenReleaseEarningsCommandIsInvokedWithCorrectKeys()
    {
        // Arrange
        var learningKey = Guid.NewGuid();
        var learnerKey = Guid.NewGuid();
        var learnerRef = "L-REF-001";
        
        var message = new ApprenticeshipPayableEarningsUpdatedEvent
        {
            LearnerKey = learnerKey,
            LearnerRef = learnerRef,
            LearningKey = learningKey
        };
        
        var handler = new MessageHandlers.Handlers.ApprenticeshipPayableEarningsUpdatedEventHandler(
            _mockCommandHandler.Object,
            Mock.Of<ILogger<MessageHandlers.Handlers.ApprenticeshipPayableEarningsUpdatedEventHandler>>());

        // Act
        await handler.Handle(message, _mockContext.Object);

        // Assert
        _mockCommandHandler.Verify(x => x.Handle(
            It.Is<ReleaseEarningsCommand.ReleaseEarningsCommand>(c =>
                c.LearningKey == learningKey),
            It.IsAny<CancellationToken>()), Times.Once);
    }
}