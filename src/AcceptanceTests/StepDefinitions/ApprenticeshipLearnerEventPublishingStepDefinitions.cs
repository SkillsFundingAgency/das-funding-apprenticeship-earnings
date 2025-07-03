using SFA.DAS.Learning.Types;
using SFA.DAS.Funding.ApprenticeshipEarnings.AcceptanceTests.Constants;
using SFA.DAS.Funding.ApprenticeshipEarnings.AcceptanceTests.Extensions;
using SFA.DAS.Funding.ApprenticeshipEarnings.AcceptanceTests.Model;
using SFA.DAS.Funding.ApprenticeshipEarnings.DataAccess.Entities;
using SFA.DAS.Funding.ApprenticeshipEarnings.TestHelpers;
using TechTalk.SpecFlow.Assist;
using FundingPlatform = SFA.DAS.Learning.Enums.FundingPlatform;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.AcceptanceTests.StepDefinitions;

[Binding]
public class LearningCreatedEventPublishingStepDefinitions
{
    private readonly ScenarioContext _scenarioContext;
    private readonly TestContext _testContext;

    public LearningCreatedEventPublishingStepDefinitions(ScenarioContext scenarioContext, TestContext testContext)
    {
        _scenarioContext = scenarioContext;
        _testContext = testContext;
    }

    [Given(@"the apprenticeship learner is 16-18 at the start of the apprenticeship")]
    public void GivenTheApprenticeshipIsUnder19()
    {
        TestSystemClock.SetDateTime(new DateTime(2020, 09, 01));

        _scenarioContext.GetLearningCreatedEventBuilder()
            .WithStartDate(new DateTime(2020, 8, 1))
            .WithDateOfBirth(new DateTime(2002, 9, 1))
            .WithAgeAtStart(18);
    }

    [Given(@"the apprenticeship learner is 19 plus at the start of the apprenticeship")]
    public void GivenTheApprenticeshipIsOver19()
    {
        TestSystemClock.SetDateTime(new DateTime(2020, 09, 01));

        _scenarioContext.GetLearningCreatedEventBuilder()
            .WithStartDate(new DateTime(2020, 8, 1))
            .WithDateOfBirth(new DateTime(2000, 9, 1))
            .WithAgeAtStart(19);
    }

    [Given(@"An apprenticeship starts on (.*) and ends on (.*)")]
    public void GivenTheApprenticeshipStartsOnAndEndsOn(DateTime startDate, DateTime endDate)
    {
        TestSystemClock.SetDateTime(startDate.AddMonths(1));
        _scenarioContext.GetLearningCreatedEventBuilder()
            .WithStartDate(startDate)
            .WithEndDate(endDate);
    }

    [Given(@"An apprenticeship has been created as part of the approvals journey")]
    [Given(@"An apprenticeship has been created")]
    [Given(@"the apprenticeship commitment is approved")]
    [When(@"the apprenticeship commitment is approved")]
    [Given(@"the earnings for the apprenticeship are calculated")]
    public async Task PublishLearningCreatedEvent()
    {
        var learningCreatedEvent = _scenarioContext.GetLearningCreatedEventBuilder().Build();

        await _testContext.TestFunction.PublishEvent(learningCreatedEvent);
        _scenarioContext.Set(learningCreatedEvent);

        _scenarioContext[ContextKeys.ExpectedDeliveryPeriodLearningAmount] = EventBuilderSharedDefaults.ExpectedDeliveryPeriodLearningAmount;

        await _testContext.TestInnerApi.PublishEvent(learningCreatedEvent);
    }

    [Given("An apprenticeship not on the pilot has been created as part of the approvals journey")]
    public async Task PublishNonPilotLearningCreatedEvent()
    {
        var learningCreatedEvent = _scenarioContext.GetLearningCreatedEventBuilder()
            .WithFundingPlatform(FundingPlatform.SLD)
            .Build();

        await _testContext.TestFunction.PublishEvent(learningCreatedEvent);
        _scenarioContext.Set(learningCreatedEvent);
    }

    [When(@"the adjusted price has been calculated")]
    public async Task WhenTheAdjustedPriceHasBeenCalculated()
    {
        await WaitHelper.WaitForItAsync(async () => await EnsureApprenticeshipExists(), "Failed to create Apprenticeship");
    }

    [Then(@"the total completion payment amount of 20% of the adjusted price must be calculated")]
    public async Task ThenTheCompletionPaymentAmountIsCalculated()
    {
        var entity = await GetApprenticeshipEntity();
        var currentEpisode = entity.GetCurrentEpisode(TestSystemClock.Instance());
        var learningCreatedEvent = _scenarioContext.Get<LearningCreatedEvent>();
        currentEpisode.EarningsProfile.CompletionPayment.Should().Be(learningCreatedEvent.Episode.Prices.First().TotalPrice* .2m);
    }

    [Given("An apprenticeship has been created as part of the approvals journey with a funding band maximum lower than the agreed price")]
    public async Task PublishApprenticeshipLearnerEventFundingBandCapScenario()
    {
        var learningCreatedEvent = _scenarioContext.GetLearningCreatedEventBuilder()
            .WithTotalPrice(35000)
            .WithFundingBandMaximum(30000)
            .Build();

        await _testContext.TestFunction.PublishEvent(learningCreatedEvent);
        _scenarioContext.Set(learningCreatedEvent);

        _scenarioContext[ContextKeys.ExpectedDeliveryPeriodLearningAmount] = 1000;
    }

    [Given(@"an apprenticeship has been created with the following information")]
    public void GivenAnApprenticeshipHasBeenCreatedWithTheFollowingInformation(Table table)
    {
        _scenarioContext.GetLearningCreatedEventBuilder()
            .WithDataFromSetupModel(table.CreateSet<ApprenticeshipCreatedSetupModel>().Single());
    }

    [Given(@"the following Price Episodes")]
    public void GivenTheFollowingPriceEpisodes(Table table)
    {
        _scenarioContext.GetLearningCreatedEventBuilder()
            .WithPricesFromSetupModels(table.CreateSet<PriceEpisodeSetupModel>().ToList());
    }

    [Given(@"earnings are calculated")]
    [Given(@"earnings have been calculated")]
    [When(@"earnings are calculated")]
    public async Task EarningsAreCalculated()
    {
        var learningCreatedEvent = _scenarioContext.GetLearningCreatedEventBuilder().Build();
        
        await _testContext.TestFunction.PublishEvent(learningCreatedEvent);
        _scenarioContext.Set(learningCreatedEvent);
    }

    private async Task<ApprenticeshipModel?> GetApprenticeshipEntity()
    {
        var learningCreatedEvent = _scenarioContext.Get<LearningCreatedEvent>();
        return await _testContext.SqlDatabase.GetApprenticeship(learningCreatedEvent.LearningKey);
    }

    private async Task<bool> EnsureApprenticeshipExists()
    {
        var apprenticeshipEntity = await GetApprenticeshipEntity();

        if (apprenticeshipEntity == null)
        {
            return false;
        }

        return true;
    }
}