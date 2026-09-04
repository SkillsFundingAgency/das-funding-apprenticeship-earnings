using SFA.DAS.Funding.ApprenticeshipEarnings.AcceptanceTests.Constants;
using SFA.DAS.Funding.ApprenticeshipEarnings.AcceptanceTests.Extensions;
using SFA.DAS.Funding.ApprenticeshipEarnings.AcceptanceTests.Model;
using SFA.DAS.Funding.ApprenticeshipEarnings.DataAccess.Entities;
using SFA.DAS.Funding.ApprenticeshipEarnings.DataAccess.Entities.Apprenticeship;
using SFA.DAS.Funding.ApprenticeshipEarnings.TestHelpers;
using SFA.DAS.Learning.Types;
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

    [Given(@"an apprenticeship has been created as part of the approvals journey")]
    [Given(@"an apprenticeship has been created")]
    [Given(@"the apprenticeship commitment is approved")]
    [When(@"the apprenticeship commitment is approved")]
    [Given(@"the earnings for the apprenticeship are calculated")]
    public async Task PublishLearningCreatedEvent()
    {
        var learningCreatedEvent = _scenarioContext.GetLearningCreatedEventBuilder().Build();

        var request = learningCreatedEvent.ToCreateUnapprovedApprenticeshipLearningRequest(_testContext.FundingBandMaximumService.GetFundingBandMaximum());
        await _testContext.TestInnerApi.Post("/learning", request);

        _scenarioContext.Set(learningCreatedEvent);

        _scenarioContext[ContextKeys.ExpectedDeliveryPeriodLearningAmount] = EventBuilderSharedDefaults.ExpectedDeliveryPeriodLearningAmount;

        await ApproveLearning(learningCreatedEvent);
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
        var entity = await GetApprenticeshipLearningEntity();
        var currentEpisode = entity.GetCurrentEpisode(TestSystemClock.Instance());
        var learningCreatedEvent = _scenarioContext.Get<LearningCreatedEvent>();
        currentEpisode.EarningsProfile.CompletionPayment.Should().Be(learningCreatedEvent.Episode.Prices.First().TotalPrice * .2m);
    }

    [Given(@"an apprenticeship has been created with the following information")]
    public void GivenAnApprenticeshipHasBeenCreatedWithTheFollowingInformation(Table table)
    {
        _scenarioContext.GetLearningCreatedEventBuilder()
            .WithDataFromSetupModel(table.CreateSet<ApprenticeshipCreatedSetupModel>().Single());
    }

    [Given(@"a funding band maximum of (.*)")]
    public void GivenTheFollowingPriceEpisodes(int fundingBandMaximum)
    {
        _testContext.FundingBandMaximumService.SetFundingBandMaximum(fundingBandMaximum);
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

        var request = learningCreatedEvent.ToCreateUnapprovedApprenticeshipLearningRequest(_testContext.FundingBandMaximumService.GetFundingBandMaximum());
        await _testContext.TestInnerApi.Post("/learning", request);

        _scenarioContext.Set(learningCreatedEvent);

        await ApproveLearning(learningCreatedEvent);
    }

    private async Task ApproveLearning(LearningCreatedEvent learningCreatedEvent)
    {
        var learningApprovedEvent = new LearningApprovedEvent
        {
            LearningKey = learningCreatedEvent.LearningKey,
            EpisodeKey = learningCreatedEvent.Episode.Key,
            ApprovalsApprenticeshipId = _scenarioContext.GetApprovalsApprenticeshipId(),
            EmployerAccountId = _scenarioContext.GetEmployerAccountId(),
            FundingAccountId = _scenarioContext.GetFundingAccountId(),
            LearnerKey = _scenarioContext.GetLearnerKey(),
            LearnerRef = _scenarioContext.GetLearnerRef(),
            EmployerType = _scenarioContext.GetEmployerType()
        };

        await _testContext.TestFunction.PublishEvent(learningApprovedEvent);
    }

    private async Task<ApprenticeshipLearningEntity?> GetApprenticeshipLearningEntity()
    {
        var learningCreatedEvent = _scenarioContext.Get<LearningCreatedEvent>();
        return await _testContext.SqlDatabase.GetApprenticeshipLearning(learningCreatedEvent.LearningKey);
    }

    private async Task<bool> EnsureApprenticeshipExists()
    {
        var apprenticeshipEntity = await GetApprenticeshipLearningEntity();

        if (apprenticeshipEntity == null)
        {
            return false;
        }

        return true;
    }
}