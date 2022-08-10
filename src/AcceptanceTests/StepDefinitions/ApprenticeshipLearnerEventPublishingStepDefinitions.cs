using NServiceBus;
using SFA.DAS.Funding.ApprenticeshipEarnings.DurableEntities;
using SFA.DAS.Apprenticeships.Types;
using SFA.DAS.Funding.ApprenticeshipEarnings.AcceptanceTests.Helpers;
using QueueNames = SFA.DAS.Apprenticeships.Types.QueueNames;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.AcceptanceTests.StepDefinitions;

[Binding]
public class ApprenticeshipCreatedEventPublishingStepDefinitions
{
    private readonly ScenarioContext _scenarioContext;
    private readonly TestContext _testContext;
    private static IEndpointInstance? _endpointInstance;
    private ApprenticeshipCreatedEvent _apprenticeshipCreatedEvent;

    public ApprenticeshipCreatedEventPublishingStepDefinitions(ScenarioContext scenarioContext, TestContext testContext)
    {
        _scenarioContext = scenarioContext;
        _testContext = testContext;
    }

    [BeforeTestRun]
    public static async Task StartEndpoint()
    {
        _endpointInstance = await EndpointHelper
            .StartEndpoint(QueueNames.ApprovalCreated, true, new[] { typeof(ApprenticeshipCreatedEvent) });
    }

    [AfterTestRun]
    public static async Task StopEndpoint()
    {
        await _endpointInstance.Stop()
            .ConfigureAwait(false);
    }

    [Given(@"An apprenticeship has been created as part of the approvals journey")]
	[Given(@"the apprenticeship commitment is approved")]
    public async Task PublishApprenticeshipCreatedEvent()
    {
        _apprenticeshipCreatedEvent = new ApprenticeshipCreatedEvent
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
        };
        await _endpointInstance.Publish(_apprenticeshipCreatedEvent);

        _scenarioContext["expectedDeliveryPeriodCount"] = 24;
        _scenarioContext["expectedDeliveryPeriodLearningAmount"] = 500;
    }

    [When(@"the adjusted price has been calculated")]
    public void WhenTheAdjustedPriceHasBeenCalculated()
    {

    }

    [Then(@"the total completion payment amount of 20% of the adjusted price must be calculated")]
    public async Task ThenTheCompletionPaymentAmountIsCalculated()
    {
        var entity = await _testContext.TestFunction.GetEntity(nameof(ApprenticeshipEntity), _apprenticeshipCreatedEvent.ApprenticeshipKey.ToString());
        entity.Model.EarningsProfile.CompletionPayment.Should().Be(_apprenticeshipCreatedEvent.AgreedPrice * .2m);
    }
}