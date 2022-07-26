using NServiceBus;

namespace SFA.DAS.Funding.IntegrationTests.Infrastructure.Events
{
    public class SampleInputEvent : IEvent
    {
        public DateTime StartDate { get; set; }
    }
}
