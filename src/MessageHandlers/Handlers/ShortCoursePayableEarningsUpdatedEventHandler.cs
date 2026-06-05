using Microsoft.Extensions.Logging;
using NServiceBus;
using SFA.DAS.Funding.ApprenticeshipEarnings.Command;
using SFA.DAS.Funding.ApprenticeshipEarnings.Command.ProcessShortCoursePayableEarningsUpdatedCommand;
using SFA.DAS.Funding.ApprenticeshipEarnings.Types;
using System.Threading.Tasks;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.MessageHandlers.Handlers;

public class ShortCoursePayableEarningsUpdatedEventHandler(
    ICommandHandler<ProcessShortCoursePayableEarningsUpdatedCommand> processShortCoursePayableEarningsUpdatedCommandHandler,
    ILogger<ShortCoursePayableEarningsUpdatedEventHandler> logger)
    : IHandleMessages<ShortCoursePayableEarningsUpdatedEvent>
{
    public async Task Handle(ShortCoursePayableEarningsUpdatedEvent message, IMessageHandlerContext context)
    {
        logger.LogInformation("{functionName} processing LearningKey: {LearningKey}", nameof(ShortCoursePayableEarningsUpdatedEventHandler), message.LearningKey);

        await processShortCoursePayableEarningsUpdatedCommandHandler.Handle(new ProcessShortCoursePayableEarningsUpdatedCommand(message), context.CancellationToken);
    }
}
