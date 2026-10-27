using Microsoft.Extensions.Logging;
using NServiceBus;
using SFA.DAS.Funding.ApprenticeshipEarnings.Command;
using SFA.DAS.Funding.ApprenticeshipEarnings.Command.ApplyFurtherApprovalCommand;
using SFA.DAS.Learning.Types;
using System.Threading.Tasks;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.MessageHandlers.Handlers;

public class EpisodeApprovedEventHandler(
    ICommandHandler<ApplyFurtherApprovalCommand> applyFurtherApprovalCommandHandler,
    ILogger<EpisodeApprovedEventHandler> logger)
    : IHandleMessages<EpisodeApprovedEvent>
{
    public async Task Handle(EpisodeApprovedEvent message, IMessageHandlerContext context)
    {
        logger.LogInformation("{HandlerName} processing LearningKey: {LearningKey}, EpisodeKey: {EpisodeKey}",
            nameof(EpisodeApprovedEventHandler),
            message.LearningKey,
            message.EpisodeKey);

        await applyFurtherApprovalCommandHandler.Handle(
            new ApplyFurtherApprovalCommand(message.LearningKey, message.EpisodeKey, message.LearnerKey, message.LearnerRef),
            context.CancellationToken);
    }
}
