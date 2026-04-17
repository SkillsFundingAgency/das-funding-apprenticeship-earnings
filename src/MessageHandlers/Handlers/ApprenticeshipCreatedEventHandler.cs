using Microsoft.Extensions.Logging;
using SFA.DAS.Funding.ApprenticeshipEarnings.Command;
using SFA.DAS.Funding.ApprenticeshipEarnings.Command.CreateApprenticeshipCommand;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Models.Apprenticeship;
using SFA.DAS.Learning.Types;
using SFA.DAS.ServiceBus;
using System;
using System.Threading.Tasks;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.MessageHandlers.Handlers;

public class LearningCreatedEventHandler(
    ICommandHandler<CreateApprenticeshipCommand, ApprenticeshipLearning> createApprenticeshipCommandHandler,
    ILogger<LearningCreatedEventHandler> logger)
    : IHandleMessages<LearningCreatedEvent>
{
    public async Task Handle(LearningCreatedEvent message, IMessageHandlerContext context)
    {
        try
        {
            logger.LogInformation($"{nameof(LearningCreatedEventHandler)} processing...");

            logger.LogInformation("LearningKey: {0} Received LearningCreatedEvent", message.LearningKey);

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