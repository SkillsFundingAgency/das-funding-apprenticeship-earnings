using NServiceBus;

namespace SFA.DAS.Funding.IntegrationTests.Infrastructure.Events
{
    public class SampleOutputEvent : IEvent
    {
        public string Data { get; }

        public SampleOutputEvent(string data)
        {
            Data = data;
        }
    }
}