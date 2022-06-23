using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using NServiceBus;
using SFA.DAS.Funding.ApprenticeshipEarnings.Infrastructure;
using SFA.DAS.Funding.ApprenticeshipEarnings.InternalEvents;
using SFA.DAS.NServiceBus.AzureFunction.Attributes;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.DurableEntities
{
    public class HandleSampleEvent
    {
        [FunctionName("HandleSampleEvent")]
        public Task RunEvent([NServiceBusTrigger(Endpoint = QueueNames.PublishSampleEvent)] SampleEvent message)
        {
            Console.WriteLine("SampleEvent received");

            return Task.CompletedTask;
        }
    }

    public class PublishSampleEvent
    {
        private readonly IMessageSession _eventPublisher;

        public PublishSampleEvent(Lazy<IMessageSession> eventPublisher)
        {
            _eventPublisher = eventPublisher.Value;
        }

        [FunctionName("HttpTriggerWPublishSampleEvent")]
        public Task RunHttp([HttpTrigger(AuthorizationLevel.Function, "POST,GET", Route = "boom")] HttpRequestMessage request)
        {
            var @event = new SampleEvent();

            return _eventPublisher.Publish(@event);
        }
    }
}
