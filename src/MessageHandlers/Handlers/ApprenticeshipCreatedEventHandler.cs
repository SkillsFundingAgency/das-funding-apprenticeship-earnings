using Microsoft.Extensions.Logging;
using SFA.DAS.Learning.Types;
using SFA.DAS.Funding.ApprenticeshipEarnings.Command;
using SFA.DAS.Funding.ApprenticeshipEarnings.Command.CreateApprenticeshipCommand;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Apprenticeship;
using System;
using System.Text.Json;
using System.Threading.Tasks;
using NServiceBus;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.MessageHandlers.Handlers;

public class LearningCreatedEventHandler(
    ICommandHandler<CreateApprenticeshipCommand, Apprenticeship> createApprenticeshipCommandHandler,
    ILogger<LearningCreatedEventHandler> logger)
    : IHandleMessages<LearningCreatedEvent>
{
    public async Task Handle(LearningCreatedEvent message, IMessageHandlerContext context)
    {
        try
        {
            logger.LogInformation($"{nameof(LearningCreatedEventHandler)} processing...");

            logger.LogInformation("LearningKey: {0} Received LearningCreatedEvent: {1}",
                message.LearningKey,
                JsonSerializer.Serialize(message, new JsonSerializerOptions { WriteIndented = true }));

            if (!(message.Episode.FundingPlatform.HasValue && Enum.Parse<FundingPlatform>(message.Episode.FundingPlatform.Value.ToString()) == FundingPlatform.DAS))
            {
                logger.LogInformation($"{nameof(LearningCreatedEventHandler)} - Not generating earnings for non pilot apprenticeship with LearningKey = {message.LearningKey}");
                return;
            }

            var command = new CreateApprenticeshipCommand(message);
            await createApprenticeshipCommandHandler.Handle(command, context.CancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, $"{nameof(CreateApprenticeshipCommand)} threw exception.");
            throw;
        }
    }
}