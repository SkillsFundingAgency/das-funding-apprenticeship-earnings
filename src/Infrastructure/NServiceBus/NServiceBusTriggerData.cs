using NServiceBus.Transport;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.Infrastructure.NServiceBus
{
    public class NServiceBusTriggerData
    {
        public byte[] Data { get; set; }
        public Dictionary<string, string> Headers { get; set; }
        public IDispatchMessages Dispatcher { get; set; }
    }
}
