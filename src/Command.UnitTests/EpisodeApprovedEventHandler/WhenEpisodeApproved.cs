using Microsoft.Extensions.Logging;
using Moq;
using SFA.DAS.Learning.Types;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.Command.UnitTests.EpisodeApprovedEventHandler;

[TestFixture]
public class WhenEpisodeApproved
{
    [Test]
    public async Task ThenApplyFurtherApprovalCommandIsInvokedWithCorrectKeys()
    {
        var commandHandler = new Mock<ICommandHandler<ApplyFurtherApprovalCommand.ApplyFurtherApprovalCommand>>();
        var message = new EpisodeApprovedEvent
        {
            LearnerKey = Guid.NewGuid(),
            LearningKey = Guid.NewGuid(),
            EpisodeKey = Guid.NewGuid(),
            LearnerRef = "LEARNER-REF"
        };

        var handler = new MessageHandlers.Handlers.EpisodeApprovedEventHandler(
            commandHandler.Object,
            Mock.Of<ILogger<MessageHandlers.Handlers.EpisodeApprovedEventHandler>>());

        await handler.Handle(message, Mock.Of<IMessageHandlerContext>());

        commandHandler.Verify(x => x.Handle(
            It.Is<ApplyFurtherApprovalCommand.ApplyFurtherApprovalCommand>(c =>
                c.LearningKey == message.LearningKey &&
                c.EpisodeKey == message.EpisodeKey &&
                c.LearnerKey == message.LearnerKey &&
                c.LearnerRef == message.LearnerRef),
            It.IsAny<CancellationToken>()), Times.Once);
    }
}
