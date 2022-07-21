using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;
using SFA.DAS.Funding.ApprenticeshipEarnings.Infrastructure;
using SFA.DAS.NServiceBus.AzureFunction.Attributes;
using System;
using System.Threading.Tasks;
using NServiceBus;
using SFA.DAS.Funding.Events;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.DurableEntities
{
    public class SampleFunctions
    {
        private readonly IMessageSession _messageSession;

        public SampleFunctions(Lazy<IMessageSession> messageSession)
        {
            _messageSession = messageSession.Value;
        }

        [FunctionName(nameof(SampleFunctionEventHandler))]
        public async Task SampleFunctionEventHandler(
            [NServiceBusTrigger(Endpoint = QueueNames.ApprenticeshipLearners)] SampleInputEvent trigger,
            [DurableClient] IDurableEntityClient client,
            ILogger log)
        {
            try
            {
                log.LogInformation("SampleFunctionEventHandler triggered!");
                log.LogInformation("StartDate: " + trigger.StartDate.ToShortDateString());

                await _messageSession.Publish(new SampleOutputEvent("Hello world!"));
            }
            catch (Exception ex)
            {
                log.LogError($"{nameof(ApprenticeshipEntity)} threw exception.", ex);
                throw;
            }
        }
    }
}