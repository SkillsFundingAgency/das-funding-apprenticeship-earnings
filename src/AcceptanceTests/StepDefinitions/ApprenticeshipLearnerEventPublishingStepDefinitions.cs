using NServiceBus;
using SFA.DAS.Apprenticeships.Types;
using SFA.DAS.Funding.ApprenticeshipEarnings.AcceptanceTests.Helpers;
using SFA.DAS.Funding.ApprenticeshipEarnings.DurableEntities;
using QueueNames = SFA.DAS.Funding.ApprenticeshipEarnings.DurableEntities.QueueNames;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.AcceptanceTests.StepDefinitions;

[Binding]
public class ApprenticeshipCreatedEventPublishingStepDefinitions
{
    private readonly ScenarioContext _scenarioContext;
    private readonly TestContext _testContext;
    private static IEndpointInstance? _endpointInstance;
    private ApprenticeshipCreatedEvent _apprenticeshipCreatedEvent;

    private DateTime _startDate = new DateTime(2019, 01, 01);
    private DateTime _dateOfBirth = new DateTime(2000, 1, 1);
    private int _ageAtStartOfApprenticeship = 21;
    private Random _random = new();

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

    [Given(@"the apprenticeship learner is 16-18 at the start of the apprenticeship")]
    public void GivenTheApprenticeshipIsUnder19()
    {
        _startDate = new DateTime(2020, 8, 1);
        _dateOfBirth = new DateTime(2002, 9, 1);
        _ageAtStartOfApprenticeship = 18;
    }

    [Given(@"the apprenticeship learner is 19 plus at the start of the apprenticeship")]
    public void GivenTheApprenticeshipIsOver19()
    {
        _startDate = new DateTime(2020, 8, 1);
        _dateOfBirth = new DateTime(2000, 9, 1);
        _ageAtStartOfApprenticeship = 19;
    }

    [Given(@"An apprenticeship has been created as part of the approvals journey")]
    [Given(@"the apprenticeship commitment is approved")]
    [When(@"the apprenticeship commitment is approved")]
    public async Task PublishApprenticeshipCreatedEvent()
    {
        _apprenticeshipCreatedEvent = new ApprenticeshipCreatedEvent
        {
            AgreedPrice = 15000,
            ActualStartDate = _startDate,
            ApprenticeshipKey = Guid.NewGuid(),
            EmployerAccountId = 114,
            FundingType = FundingType.Levy,
            PlannedEndDate = new DateTime(2021, 1, 1),
            UKPRN = 116,
            TrainingCode = "AbleSeafarer",
            FundingEmployerAccountId = null,
            Uln = _random.Next().ToString(),
            LegalEntityName = "MyTrawler",
            ApprovalsApprenticeshipId = 120,
            DateOfBirth = _dateOfBirth,
            FundingBandMaximum = 15000,
            AgeAtStartOfApprenticeship = _ageAtStartOfApprenticeship,
            FundingPlatform = FundingPlatform.DAS
        };
        await _endpointInstance.Publish(_apprenticeshipCreatedEvent);

        _scenarioContext[ContextKeys.ExpectedDeliveryPeriodCount] = 24;
        _scenarioContext[ContextKeys.ExpectedDeliveryPeriodLearningAmount] = 500;
        _scenarioContext[ContextKeys.ExpectedUln] = _apprenticeshipCreatedEvent.Uln;
    }

    [Given("An apprenticeship not on the pilot has been created as part of the approvals journey")]
    public async Task PublishNonPilotApprenticeshipCreatedEvent()
    {
        _apprenticeshipCreatedEvent = new ApprenticeshipCreatedEvent
        {
            AgreedPrice = 15000,
            ActualStartDate = _startDate,
            ApprenticeshipKey = Guid.NewGuid(),
            EmployerAccountId = 114,
            FundingType = FundingType.Levy,
            PlannedEndDate = new DateTime(2020, 12, 31),
            UKPRN = 116,
            TrainingCode = "AbleSeafarer",
            FundingEmployerAccountId = null,
            Uln = _random.Next().ToString(),
            LegalEntityName = "MyTrawler",
            ApprovalsApprenticeshipId = 120,
            DateOfBirth = _dateOfBirth,
            FundingBandMaximum = 15000,
            AgeAtStartOfApprenticeship = _ageAtStartOfApprenticeship,
            FundingPlatform = FundingPlatform.SLD
        };
        await _endpointInstance.Publish(_apprenticeshipCreatedEvent);

        _scenarioContext[ContextKeys.ExpectedUln] = _apprenticeshipCreatedEvent.Uln;
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

    [Given("An apprenticeship has been created as part of the approvals journey with a funding band maximum lower than the agreed price")]
    public async Task PublishApprenticeshipLearnerEventFundingBandCapScenario()
    {
        _apprenticeshipCreatedEvent = new ApprenticeshipCreatedEvent
        {
            AgreedPrice = 35000,
            ActualStartDate = new DateTime(2019, 01, 01),
            ApprenticeshipKey = Guid.NewGuid(),
            EmployerAccountId = 114,
            FundingType = FundingType.Levy,
            PlannedEndDate = new DateTime(2020, 12, 31),
            UKPRN = 116,
            TrainingCode = "AbleSeafarer",
            FundingEmployerAccountId = null,
            Uln = _random.Next().ToString(),
            LegalEntityName = "MyTrawler",
            ApprovalsApprenticeshipId = 120,
            FundingBandMaximum = 30000,
            FundingPlatform = FundingPlatform.DAS
        };
        await _endpointInstance.Publish(_apprenticeshipCreatedEvent);

        _scenarioContext[ContextKeys.ExpectedDeliveryPeriodCount] = 24;
        _scenarioContext[ContextKeys.ExpectedDeliveryPeriodLearningAmount] = 1000;
        _scenarioContext[ContextKeys.ExpectedUln] = _apprenticeshipCreatedEvent.Uln;
    }
}