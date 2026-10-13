using Microsoft.Extensions.Logging;
using NServiceBus;
using SFA.DAS.Funding.ApprenticeshipEarnings.Command;
using SFA.DAS.Funding.ApprenticeshipEarnings.Command.ReleaseEarningsCommand;
using SFA.DAS.Funding.ApprenticeshipEarnings.Types;
using System.Threading.Tasks;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.MessageHandlers.Handlers;

public class ApprenticeshipPayableEarningsUpdatedEventHandler(
    ICommandHandler<ReleaseEarningsCommand> releaseEarningsCommandHandler,
    ILogger<ApprenticeshipPayableEarningsUpdatedEventHandler> logger)
    : IHandleMessages<ApprenticeshipPayableEarningsUpdatedEvent>
{
    public async Task Handle(ApprenticeshipPayableEarningsUpdatedEvent message, IMessageHandlerContext context)
    {
        logger.LogInformation("{HandlerName} processing LearningKey: {LearningKey}", nameof(ApprenticeshipPayableEarningsUpdatedEventHandler), message.LearningKey);

        var request = new ReleaseEarningsRequest
        {
            LearnerKey = message.LearnerKey,
            LearnerRef = message.LearnerRef
        };

        await releaseEarningsCommandHandler.Handle(new ReleaseEarningsCommand(message.LearningKey, request), context.CancellationToken);
    }
}
