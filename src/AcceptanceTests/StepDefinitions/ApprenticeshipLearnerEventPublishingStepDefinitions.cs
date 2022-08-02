using NServiceBus;
using SFA.DAS.Apprenticeships.Types;
using SFA.DAS.NServiceBus.Configuration.NewtonsoftJsonSerializer;
using QueueNames = SFA.DAS.Funding.ApprenticeshipEarnings.Types.QueueNames;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.AcceptanceTests.StepDefinitions;

[Binding]
public class ApprenticeshipCreatedEventPublishingStepDefinitions
{
    private readonly ScenarioContext _scenarioContext;
    private static IEndpointInstance? _endpointInstance;

    public ApprenticeshipCreatedEventPublishingStepDefinitions(ScenarioContext scenarioContext)
    {
        _scenarioContext = scenarioContext;
    }

    [BeforeTestRun]
    public static async Task StartEndpoint()
    {
        var endpointConfiguration = new EndpointConfiguration(QueueNames.ApprenticeshipCreated);
        endpointConfiguration.AssemblyScanner().ThrowExceptions = false;
        endpointConfiguration.SendOnly();
        endpointConfiguration.UseNewtonsoftJsonSerializer();
        endpointConfiguration.Conventions().DefiningEventsAs(x => x == typeof(ApprenticeshipCreatedEvent));

        var transport = endpointConfiguration.UseTransport<LearningTransport>();
        transport.StorageDirectory(Path.Combine(Directory.GetCurrentDirectory().Substring(0, Directory.GetCurrentDirectory().IndexOf("src")), @"src\.learningtransport"));

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
            Uln = "118",
            LegalEntityName = "MyTrawler",
            ApprovalsApprenticeshipId = 120
        });

        _scenarioContext["expectedDeliveryPeriodCount"] = 24;
        _scenarioContext["expectedDeliveryPeriodLearningAmount"] = 500;
    }
}