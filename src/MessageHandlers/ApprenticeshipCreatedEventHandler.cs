using System;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using SFA.DAS.Apprenticeships.Types;
using SFA.DAS.Funding.ApprenticeshipEarnings.Command.CreateApprenticeshipCommand;
using SFA.DAS.NServiceBus.AzureFunction.Attributes;
using ILogger = Microsoft.Extensions.Logging.ILogger;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.MessageHandlers;

public class ApprenticeshipCreatedEventHandler
{
    private readonly ICreateApprenticeshipCommandHandler _createApprenticeshipCommandHandler;

    public ApprenticeshipCreatedEventHandler(ICreateApprenticeshipCommandHandler createApprenticeshipCommandHandler)
    {
        _createApprenticeshipCommandHandler = createApprenticeshipCommandHandler;
    }

    [FunctionName(nameof(ApprenticeshipLearnerEventServiceBusTrigger))]
    public async Task ApprenticeshipLearnerEventServiceBusTrigger(
        [NServiceBusTrigger(Endpoint = QueueNames.ApprovalCreated)] ApprenticeshipCreatedEvent apprenticeshipCreatedEvent,
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
            await _createApprenticeshipCommandHandler.Create(command);
        }
        catch (Exception ex)
        {
            log.LogError(ex, $"{nameof(CreateApprenticeshipCommand)} threw exception.");
            throw;
        }
    }
}