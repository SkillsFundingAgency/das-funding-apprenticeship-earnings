using Microsoft.Azure.WebJobs.Description;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.Infrastructure.NServiceBus
{
    [AttributeUsage(AttributeTargets.Parameter)]
    [Binding]
    public sealed class NServiceBusTriggerAttribute : Attribute
    {
        public string Endpoint { get; set; }
        public string Connection { get; set; }
        public string LearningTransportStorageDirectory { get; set; }
    }
}
