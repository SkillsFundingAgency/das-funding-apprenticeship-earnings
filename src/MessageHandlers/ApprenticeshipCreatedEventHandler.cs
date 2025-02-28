using Microsoft.Extensions.Logging;
using SFA.DAS.Apprenticeships.Types;
using SFA.DAS.Funding.ApprenticeshipEarnings.Command;
using SFA.DAS.Funding.ApprenticeshipEarnings.Command.CreateApprenticeshipCommand;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Apprenticeship;
using System;
using System.Text.Json;
using System.Threading.Tasks;
using NServiceBus;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.MessageHandlers;

public class ApprenticeshipCreatedEventHandler(
    ICommandHandler<CreateApprenticeshipCommand, Apprenticeship> createApprenticeshipCommandHandler,
    ILogger<ApprenticeshipCreatedEventHandler> logger)
    : IHandleMessages<ApprenticeshipCreatedEvent>
{
    public async Task Handle(ApprenticeshipCreatedEvent message, IMessageHandlerContext context)
    {
        try
        {
            logger.LogInformation($"{nameof(ApprenticeshipCreatedEventHandler)} processing...");

            logger.LogInformation("ApprenticeshipKey: {0} Received ApprenticeshipCreatedEvent: {1}",
                message.ApprenticeshipKey,
                JsonSerializer.Serialize(message, new JsonSerializerOptions { WriteIndented = true }));

            if (!(message.Episode.FundingPlatform.HasValue && Enum.Parse<FundingPlatform>(message.Episode.FundingPlatform.Value.ToString()) == FundingPlatform.DAS))
            {
                logger.LogInformation($"{nameof(ApprenticeshipCreatedEventHandler)} - Not generating earnings for non pilot apprenticeship with ApprenticeshipKey = {message.ApprenticeshipKey}");
                return;
            }

            var command = new CreateApprenticeshipCommand(message);
            await createApprenticeshipCommandHandler.Handle(command);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, $"{nameof(CreateApprenticeshipCommand)} threw exception.");
            throw;
        }
    }
}