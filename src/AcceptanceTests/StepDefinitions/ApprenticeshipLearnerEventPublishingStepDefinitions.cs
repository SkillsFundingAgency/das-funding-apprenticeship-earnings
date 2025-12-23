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

    [Given(@"an apprenticeship has been created as part of the approvals journey")]
    [Given(@"an apprenticeship has been created")]
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