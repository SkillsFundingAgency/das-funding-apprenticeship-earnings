using NServiceBus;
using SFA.DAS.Funding.ApprenticeshipEarnings.Infrastructure.NServiceBus;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.AcceptanceTests.Helpers
{
    public static class EndpointHelper
    {
        public static async Task<IEndpointInstance> StartEndpoint(string endpointName, bool isSendOnly, Type[] types)
        {
            var endpointConfiguration = new EndpointConfiguration(endpointName);
            endpointConfiguration.AssemblyScanner().ThrowExceptions = false;

            if(isSendOnly) endpointConfiguration.SendOnly();

            endpointConfiguration.UseNewtonsoftJsonSerializer();
            endpointConfiguration.Conventions().DefiningEventsAs(types.Contains);

            var transport = endpointConfiguration.UseTransport<LearningTransport>();
            transport.StorageDirectory(Path.Combine(Directory.GetCurrentDirectory().Substring(0, Directory.GetCurrentDirectory().IndexOf("src")), @"src\.learningtransport"));

            return await Endpoint.Start(endpointConfiguration)
                .ConfigureAwait(false);
        }
    }
}
