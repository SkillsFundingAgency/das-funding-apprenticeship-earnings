using Microsoft.Extensions.Logging;
using NServiceBus;
using SFA.DAS.Funding.ApprenticeshipEarnings.Command;
using SFA.DAS.Funding.ApprenticeshipEarnings.Command.UpdateEarningsQueryCommand;
using SFA.DAS.Funding.ApprenticeshipEarnings.Types;
using System;
using System.Threading.Tasks;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.MessageHandlers.Handlers;

public class EarningsGeneratedEventHandler(
    ICommandHandler<UpdateEarningsQueryCommand> commandHandler,
    ILogger<EarningsGeneratedEvent> logger)
    : IHandleMessages<EarningsGeneratedEvent>
{
    public async Task Handle(EarningsGeneratedEvent message, IMessageHandlerContext context)
    {
        try
        {
            logger.LogInformation($"{nameof(EarningsGeneratedEventHandler)} processing ApprenticeshipKey {message.ApprenticeshipKey}");
            
            var command = new UpdateEarningsQueryCommand(UpdateEarningsQueryType.Create, message.ApprenticeshipKey);

            await commandHandler.Handle(command, context.CancellationToken);

            logger.LogInformation($"{nameof(EarningsGeneratedEventHandler)} successfully processed ApprenticeshipKey {message.ApprenticeshipKey}");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, $"{nameof(EarningsGeneratedEventHandler)} threw exception while processing ApprenticeshipKey {message.ApprenticeshipKey}");
            throw;
        }
    }
}

public class ApprenticeshipEarningsRecalculatedEventHandler(
    ICommandHandler<UpdateEarningsQueryCommand> commandHandler,
    ILogger<ApprenticeshipEarningsRecalculatedEventHandler> logger)
    : IHandleMessages<ApprenticeshipEarningsRecalculatedEvent>
{
    public async Task Handle(ApprenticeshipEarningsRecalculatedEvent message, IMessageHandlerContext context)
    {
        try
        {
            logger.LogInformation($"{nameof(ApprenticeshipEarningsRecalculatedEventHandler)} processing ApprenticeshipKey {message.ApprenticeshipKey}");
            var command = new UpdateEarningsQueryCommand(UpdateEarningsQueryType.Recalculate, message.ApprenticeshipKey);
            await commandHandler.Handle(command, context.CancellationToken);
            logger.LogInformation($"{nameof(ApprenticeshipEarningsRecalculatedEventHandler)} successfully processed ApprenticeshipKey {message.ApprenticeshipKey}");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, $"{nameof(ApprenticeshipEarningsRecalculatedEventHandler)} threw exception while processing ApprenticeshipKey {message.ApprenticeshipKey}");
            throw;
        }
    }
}
