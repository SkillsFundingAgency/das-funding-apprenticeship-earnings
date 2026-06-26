using Microsoft.Extensions.Logging;
using Moq;
using SFA.DAS.Learning.Types;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.Command.UnitTests.LearningApprovedEventHandler;

[TestFixture]
public class WhenLearningApproved
{
    private Mock<ICommandHandler<ApproveLearningCommand.ApproveLearningCommand>> _mockCommandHandler = null!;
    private Mock<IMessageHandlerContext> _mockContext = null!;

    [SetUp]
    public void SetUp()
    {
        _mockCommandHandler = new Mock<ICommandHandler<ApproveLearningCommand.ApproveLearningCommand>>();
        _mockContext = new Mock<IMessageHandlerContext>();
    }

    [Test]
    public async Task ThenApproveLearningCommandIsInvokedWithCorrectKeys()
    {
        // Arrange
        var learningKey = Guid.NewGuid();
        var learnerKey = Guid.NewGuid();
        var learnerRef = "L-REF-001";
        var approvalsApprenticeshipId = 15L;
        var employerAccountId = 112;
        var fundingAccountId = 223;

        var message = new LearningApprovedEvent { LearningKey = learningKey, LearnerKey = learnerKey, LearnerRef = learnerRef, ApprovalsApprenticeshipId = approvalsApprenticeshipId, EmployerAccountId = employerAccountId, FundingAccountId = fundingAccountId };

        var handler = new MessageHandlers.Handlers.LearningApprovedEventHandler(
            _mockCommandHandler.Object,
            Mock.Of<ILogger<MessageHandlers.Handlers.LearningApprovedEventHandler>>());

        // Act
        await handler.Handle(message, _mockContext.Object);

        // Assert
        _mockCommandHandler.Verify(x => x.Handle(
            It.Is<ApproveLearningCommand.ApproveLearningCommand>(c =>
                c.LearningKey == learningKey &&
                c.LearnerKey == learnerKey &&
                c.LearnerRef == learnerRef &&
                c.EmployerAccountId == employerAccountId &&
                c.FundingAccountId == fundingAccountId &&
                c.ApprovalsApprenticeshipId == approvalsApprenticeshipId),
            It.IsAny<CancellationToken>()), Times.Once);
    }
}
