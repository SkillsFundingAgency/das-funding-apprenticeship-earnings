using Microsoft.Extensions.Logging;
using NServiceBus;
using SFA.DAS.Funding.ApprenticeshipEarnings.Command;
using SFA.DAS.Funding.ApprenticeshipEarnings.Command.SendApprenticeshipPayableEarningsToPaymentsCommand;
using SFA.DAS.Funding.ApprenticeshipEarnings.Types;
using System.Threading.Tasks;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.MessageHandlers.Handlers;

public class ApprenticeshipPayableEarningsUpdatedEventHandler(
    ICommandHandler<SendApprenticeshipPayableEarningsToPaymentsCommand> sendApprenticeshipPayableEarningsToPaymentsCommandHandler,
    ILogger<ApprenticeshipPayableEarningsUpdatedEventHandler> logger)
    : IHandleMessages<ApprenticeshipPayableEarningsUpdatedEvent>
{
    public async Task Handle(ApprenticeshipPayableEarningsUpdatedEvent message, IMessageHandlerContext context)
    {
        logger.LogInformation("{FunctionName} processing LearningKey: {LearningKey}", nameof(ApprenticeshipPayableEarningsUpdatedEventHandler), message.LearningKey);

        await sendApprenticeshipPayableEarningsToPaymentsCommandHandler.Handle(new SendApprenticeshipPayableEarningsToPaymentsCommand(message), context.CancellationToken);
    }
}
