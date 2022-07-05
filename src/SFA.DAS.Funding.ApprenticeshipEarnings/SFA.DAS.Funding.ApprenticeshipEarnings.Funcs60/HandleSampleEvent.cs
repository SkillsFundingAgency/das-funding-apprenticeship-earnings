using System;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using SFA.DAS.Funding.Events;
using SFA.DAS.NServiceBus.AzureFunction.Attributes;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.Funcs60
{
    public class HandleSampleEvent
    {
        [FunctionName("HandleSampleEvent")]
        public Task RunEvent([NServiceBusTrigger(Endpoint = QueueNames.Sandbox)] SampleFundingEvent message)
        {
            Console.WriteLine($"SampleEvent received: {message.CorrelationId}");

            return Task.CompletedTask;
        }
    }
}
