using NServiceBus;
using SFA.DAS.Funding.ApprenticeshipEarnings.Infrastructure;
using SFA.DAS.Funding.ApprenticeshipEarnings.InternalEvents;
using SFA.DAS.NServiceBus.Configuration.NewtonsoftJsonSerializer;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.Acceptance.StepDefinitions;

[Binding]
public class ApprenticeshipLearnerEventPublishingStepDefinitions
{
    private static IEndpointInstance _endpointInstance;

    [BeforeTestRun]
    public static async Task StartEndpoint()
    {
        var endpointConfiguration = new EndpointConfiguration(QueueNames.ApprenticeshipLearners);
        endpointConfiguration.AssemblyScanner().ThrowExceptions = false;
        endpointConfiguration.SendOnly();
        endpointConfiguration.UseNewtonsoftJsonSerializer();

        var transport = endpointConfiguration.UseTransport<LearningTransport>();
        transport.StorageDirectory("C:\\temp\\LearningTransport\\FPAY-14");

        _endpointInstance = await Endpoint.Start(endpointConfiguration)
            .ConfigureAwait(false);
    }

    [AfterTestRun]
    public static async Task StopEndpoint()
    {
        await _endpointInstance.Stop()
            .ConfigureAwait(false);
    }

    [Given(@"An apprenticeship learner event comes in from approvals")]
    public async Task PublishApprenticeshipLearnerEvent()
    {
        await _endpointInstance.Publish(new InternalApprenticeshipLearnerEvent()
            { AgreedPrice = 11000 });
    }
}