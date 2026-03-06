using Microsoft.Extensions.Logging;
using NServiceBus;
using SFA.DAS.Funding.ApprenticeshipEarnings.Command;
using SFA.DAS.Funding.ApprenticeshipEarnings.Command.ApproveLearningCommand;
using SFA.DAS.Learning.Types;
using System.Threading.Tasks;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.MessageHandlers.Handlers;

public class LearningApprovedEventHandler(
    ICommandHandler<ApproveLearningCommand> approveLearningCommandHandler,
    ILogger<LearningApprovedEventHandler> logger)
    : IHandleMessages<LearningApprovedEvent>
{
    public async Task Handle(LearningApprovedEvent message, IMessageHandlerContext context)
    {
        logger.LogInformation("{handlerName} processing LearningKey: {LearningKey}",
            nameof(LearningApprovedEventHandler),
            message.LearningKey);

        await approveLearningCommandHandler.Handle(
            new ApproveLearningCommand(message.LearningKey),
            context.CancellationToken);
    }
}
