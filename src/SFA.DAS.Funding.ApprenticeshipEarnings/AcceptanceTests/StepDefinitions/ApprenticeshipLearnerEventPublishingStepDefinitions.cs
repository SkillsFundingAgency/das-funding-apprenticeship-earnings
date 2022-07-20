using NServiceBus;
using SFA.DAS.Apprenticeships.Events;
using SFA.DAS.Funding.ApprenticeshipEarnings.Infrastructure;
using SFA.DAS.NServiceBus.Configuration.NewtonsoftJsonSerializer;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.Acceptance.StepDefinitions;

[Binding]
public class ApprenticeshipCreatedEventPublishingStepDefinitions
{
    private readonly ScenarioContext _scenarioContext;
    private static IEndpointInstance _endpointInstance;

    public ApprenticeshipCreatedEventPublishingStepDefinitions(ScenarioContext scenarioContext)
    {
        _scenarioContext = scenarioContext;
    }

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
        await _endpointInstance.Publish(new ApprenticeshipCreatedEvent
        {
            AgreedPrice = 15000,
            ActualStartDate = new DateTime(2019, 01, 01),
            ApprenticeshipKey = Guid.NewGuid(),
            EmployerAccountId = 114,
            FundingType = FundingType.Levy,
            PlannedEndDate = new DateTime(2020, 12, 31),
            UKPRN = 116,
            TrainingCode = "AbleSeafarer",
            FundingEmployerAccountId = null,
            Uln = 118,
            LegalEntityName = "MyTrawler",
            ApprovalsApprenticeshipId = 120
        });

        _scenarioContext["expectedDeliveryPeriodCount"] = 24;
        _scenarioContext["expectedDeliveryPeriodLearningAmount"] = 500;
    }
}