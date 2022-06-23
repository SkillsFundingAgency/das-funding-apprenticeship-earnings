using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using NServiceBus;
using System;
using System.Threading.Tasks;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.Funcs31
{
    public class Function1
    {
        private static IMessageSession _eventPublisher;

        public Function1(Lazy<IMessageSession> eventPublisher)
        {
            _eventPublisher = eventPublisher.Value;
        }

        [FunctionName("Function1")]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            var corId = Guid.NewGuid();
            var @event = new SampleFundingEvent {CorrelationId = corId };
            
            await _eventPublisher.Publish(@event);

            return new AcceptedResult();
        }
    }
}
