using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using SFA.DAS.Apprenticeships.Types;
using SFA.DAS.Funding.ApprenticeshipEarnings.Command;
using SFA.DAS.Funding.ApprenticeshipEarnings.Command.CreateApprenticeshipCommand;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Apprenticeship;
using System;
using System.Text.Json;
using System.Threading.Tasks;
using ILogger = Microsoft.Extensions.Logging.ILogger;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.MessageHandlers;

public class ApprenticeshipCreatedEventHandler
{
    private readonly ICommandHandler<CreateApprenticeshipCommand, Apprenticeship> _createApprenticeshipCommandHandler;

    public ApprenticeshipCreatedEventHandler(ICommandHandler<CreateApprenticeshipCommand, Apprenticeship> createApprenticeshipCommandHandler)
    {
        _createApprenticeshipCommandHandler = createApprenticeshipCommandHandler;
    }

    [Function(nameof(ApprenticeshipLearnerEventServiceBusTrigger))]
    public async Task ApprenticeshipLearnerEventServiceBusTrigger(
        [QueueTrigger(QueueNames.ApprovalCreated)] ApprenticeshipCreatedEvent apprenticeshipCreatedEvent,
        ILogger log)
    {
        try
        {
            log.LogInformation($"{nameof(ApprenticeshipLearnerEventServiceBusTrigger)} processing...");

            log.LogInformation("ApprenticeshipKey: {0} Received ApprenticeshipCreatedEvent: {1}",
                apprenticeshipCreatedEvent.ApprenticeshipKey,
                JsonSerializer.Serialize(apprenticeshipCreatedEvent, new JsonSerializerOptions { WriteIndented = true }));

            if (!(apprenticeshipCreatedEvent.Episode.FundingPlatform.HasValue && Enum.Parse<FundingPlatform>(apprenticeshipCreatedEvent.Episode.FundingPlatform.Value.ToString()) == FundingPlatform.DAS))
            {
                log.LogInformation($"{nameof(ApprenticeshipLearnerEventServiceBusTrigger)} - Not generating earnings for non pilot apprenticeship with ApprenticeshipKey = {apprenticeshipCreatedEvent.ApprenticeshipKey}");
                return;
            }

            var command = new CreateApprenticeshipCommand(apprenticeshipCreatedEvent);
            await _createApprenticeshipCommandHandler.Handle(command);
        }
        catch (Exception ex)
        {
            log.LogError(ex, $"{nameof(CreateApprenticeshipCommand)} threw exception.");
            throw;
        }
    }
}