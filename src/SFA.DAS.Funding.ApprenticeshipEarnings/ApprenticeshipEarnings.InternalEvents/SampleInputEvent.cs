using NServiceBus;

namespace SFA.DAS.Funding.Events
{
    public class SampleInputEvent : IEvent
    {
        public DateTime StartDate { get; set; }
    }
}