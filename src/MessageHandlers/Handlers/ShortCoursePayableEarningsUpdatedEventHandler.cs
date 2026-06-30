using Microsoft.Extensions.Logging;
using NServiceBus;
using SFA.DAS.Funding.ApprenticeshipEarnings.Command;
using SFA.DAS.Funding.ApprenticeshipEarnings.Command.SendShortCoursePayableEarningsToPaymentsCommand;
using SFA.DAS.Funding.ApprenticeshipEarnings.Types;
using System.Threading.Tasks;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.MessageHandlers.Handlers;

public class ShortCoursePayableEarningsUpdatedEventHandler(
    ICommandHandler<SendShortCoursePayableEarningsToPaymentsCommand> sendShortCoursePayableEarningsToPaymentsCommandHandler,
    ILogger<ShortCoursePayableEarningsUpdatedEventHandler> logger)
    : IHandleMessages<ShortCoursePayableEarningsUpdatedEvent>
{
    public async Task Handle(ShortCoursePayableEarningsUpdatedEvent message, IMessageHandlerContext context)
    {
        logger.LogInformation("{FunctionName} processing LearningKey: {LearningKey}", nameof(ShortCoursePayableEarningsUpdatedEventHandler), message.LearningKey);

        await sendShortCoursePayableEarningsToPaymentsCommandHandler.Handle(new SendShortCoursePayableEarningsToPaymentsCommand(message), context.CancellationToken);
    }
}
