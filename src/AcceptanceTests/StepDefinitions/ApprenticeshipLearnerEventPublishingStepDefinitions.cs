using SFA.DAS.Apprenticeships.Types;
using SFA.DAS.Funding.ApprenticeshipEarnings.AcceptanceTests.Model;
using SFA.DAS.Funding.ApprenticeshipEarnings.Command.SaveCareDetailsCommand;
using SFA.DAS.Funding.ApprenticeshipEarnings.DataAccess.Entities;
using SFA.DAS.Funding.ApprenticeshipEarnings.TestHelpers;
using SFA.DAS.Funding.ApprenticeshipEarnings.Types;
using TechTalk.SpecFlow.Assist;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.AcceptanceTests.StepDefinitions;

[Binding]
public class ApprenticeshipCreatedEventPublishingStepDefinitions
{
    private readonly ScenarioContext _scenarioContext;
    private readonly TestContext _testContext;
    private ApprenticeshipCreatedEvent _apprenticeshipCreatedEvent;

    private DateTime _startDate = new DateTime(2019, 01, 01);
    private DateTime _dateOfBirth = new DateTime(2000, 1, 1);
    private int _ageAtStartOfApprenticeship = 21;
    private Random _random = new();

    private readonly DateTime _defaultCurrentDateTime = new DateTime(2020, 01, 01);

    public ApprenticeshipCreatedEventPublishingStepDefinitions(ScenarioContext scenarioContext, TestContext testContext)
    {
        _scenarioContext = scenarioContext;
        _testContext = testContext;
        TestSystemClock.SetDateTime(_defaultCurrentDateTime);
    }

    [Given(@"the apprenticeship learner is 16-18 at the start of the apprenticeship")]
    public void GivenTheApprenticeshipIsUnder19()
    {
        _startDate = new DateTime(2020, 8, 1);
        TestSystemClock.SetDateTime(new DateTime(2020, 09, 01));
        _dateOfBirth = new DateTime(2002, 9, 1);
        _ageAtStartOfApprenticeship = 18;
    }

    [Given(@"the apprenticeship learner is 19 plus at the start of the apprenticeship")]
    public void GivenTheApprenticeshipIsOver19()
    {
        _startDate = new DateTime(2020, 8, 1);
        TestSystemClock.SetDateTime(new DateTime(2020, 09, 01));
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
            ApprenticeshipKey = Guid.NewGuid(),
            Uln = _random.Next().ToString(),
            ApprovalsApprenticeshipId = 120,
            DateOfBirth = _dateOfBirth,
            Episode = new ApprenticeshipEpisode
            {
                Key = Guid.NewGuid(),
                Prices = new List<ApprenticeshipEpisodePrice>
                {
                    new()
                    {
                        TotalPrice = 15000,
                        StartDate = _startDate,
                        EndDate = new DateTime(2021, 1, 1),
                        FundingBandMaximum = 15000
                    }
                },
                EmployerAccountId = 114,
                FundingType = Apprenticeships.Enums.FundingType.Levy,
                Ukprn = 116,
                TrainingCode = "AbleSeafarer",
                FundingEmployerAccountId = null,
                LegalEntityName = "MyTrawler",
                FundingPlatform = Apprenticeships.Enums.FundingPlatform.DAS,
                AgeAtStartOfApprenticeship = _ageAtStartOfApprenticeship,
            }
        };
        await _testContext.TestFunction.PublishEvent(_apprenticeshipCreatedEvent);

        _scenarioContext[ContextKeys.ExpectedDeliveryPeriodCount] = 24;
        _scenarioContext[ContextKeys.ExpectedDeliveryPeriodLearningAmount] = 500;
        _scenarioContext[ContextKeys.ExpectedUln] = _apprenticeshipCreatedEvent.Uln;

        await _testContext.TestInnerApi.PublishEvent(_apprenticeshipCreatedEvent);
    }

    [Given("An apprenticeship not on the pilot has been created as part of the approvals journey")]
    public async Task PublishNonPilotApprenticeshipCreatedEvent()
    {
        _apprenticeshipCreatedEvent = new ApprenticeshipCreatedEvent
        {
            ApprenticeshipKey = Guid.NewGuid(),
            Uln = _random.Next().ToString(),
            ApprovalsApprenticeshipId = 120,
            DateOfBirth = _dateOfBirth,
            Episode = new ApprenticeshipEpisode
            {
                Key = Guid.NewGuid(),
                Prices = new List<ApprenticeshipEpisodePrice>
                {
                    new()
                    {
                        TotalPrice = 15000,
                        StartDate = _startDate,
                        EndDate = new DateTime(2020, 12, 31),
                        FundingBandMaximum = 15000
                    }
                },
                EmployerAccountId = 114,
                FundingType = Apprenticeships.Enums.FundingType.Levy,
                Ukprn = 116,
                TrainingCode = "AbleSeafarer",
                FundingEmployerAccountId = null,
                LegalEntityName = "MyTrawler",
                FundingPlatform = Apprenticeships.Enums.FundingPlatform.SLD,
                AgeAtStartOfApprenticeship = _ageAtStartOfApprenticeship,
            }
        };

        await _testContext.TestFunction.PublishEvent(_apprenticeshipCreatedEvent);

        _scenarioContext[ContextKeys.ExpectedUln] = _apprenticeshipCreatedEvent.Uln;
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
        currentEpisode.EarningsProfile.CompletionPayment.Should().Be(_apprenticeshipCreatedEvent.Episode.Prices.First().TotalPrice* .2m);
    }

    [Given("An apprenticeship has been created as part of the approvals journey with a funding band maximum lower than the agreed price")]
    public async Task PublishApprenticeshipLearnerEventFundingBandCapScenario()
    {
        _apprenticeshipCreatedEvent = new ApprenticeshipCreatedEvent
        {
            ApprenticeshipKey = Guid.NewGuid(),
            Uln = _random.Next().ToString(),
            ApprovalsApprenticeshipId = 120,
            DateOfBirth = _dateOfBirth,
            Episode = new ApprenticeshipEpisode
            {
                Key = Guid.NewGuid(),
                Prices = new List<ApprenticeshipEpisodePrice>
                {
                    new()
                    {
                        TotalPrice = 35000,
                        StartDate = new DateTime(2019, 01, 01),
                        EndDate = new DateTime(2020, 12, 31),
                        FundingBandMaximum = 30000
                    }
                },
                EmployerAccountId = 114,
                FundingType = Apprenticeships.Enums.FundingType.Levy,
                Ukprn = 116,
                TrainingCode = "AbleSeafarer",
                FundingEmployerAccountId = null,
                LegalEntityName = "MyTrawler",
                FundingPlatform = Apprenticeships.Enums.FundingPlatform.DAS,
                AgeAtStartOfApprenticeship = _ageAtStartOfApprenticeship,
            }
        };
        await _testContext.TestFunction.PublishEvent(_apprenticeshipCreatedEvent);

        _scenarioContext[ContextKeys.ExpectedDeliveryPeriodCount] = 24;
        _scenarioContext[ContextKeys.ExpectedDeliveryPeriodLearningAmount] = 1000;
        _scenarioContext[ContextKeys.ExpectedUln] = _apprenticeshipCreatedEvent.Uln;
    }

    [Given(@"an apprenticeship has been created with the following information")]
    public void GivenAnApprenticeshipHasBeenCreatedWithTheFollowingInformation(TechTalk.SpecFlow.Table table)
    {
        var data = table.CreateSet<ApprenticeshipCreatedSetupModel>().Single();

        _apprenticeshipCreatedEvent = new ApprenticeshipCreatedEvent
        {
            ApprenticeshipKey = Guid.NewGuid(),
            Uln = _random.Next().ToString(),
            ApprovalsApprenticeshipId = 120,
            DateOfBirth = _dateOfBirth,
            Episode = new ApprenticeshipEpisode
            {
                Key = Guid.NewGuid(),
                Prices = [],
                EmployerAccountId = 114,
                FundingType = Apprenticeships.Enums.FundingType.Levy,
                Ukprn = 116,
                TrainingCode = "AbleSeafarer",
                FundingEmployerAccountId = null,
                LegalEntityName = "MyTrawler",
                FundingPlatform = Apprenticeships.Enums.FundingPlatform.DAS,
                AgeAtStartOfApprenticeship = data.Age,
            }
        };

        _scenarioContext.Set(_apprenticeshipCreatedEvent);
    }

    [Given(@"the following Price Episodes")]
    public void GivenTheFollowingPriceEpisodes(TechTalk.SpecFlow.Table table)
    {
        var data = table.CreateSet<PriceEpisodeSetupModel>().ToList();

        _apprenticeshipCreatedEvent.Episode.Prices = data.Select(x => new ApprenticeshipEpisodePrice
        {
            TotalPrice = x.Price,
            StartDate = x.StartDate,
            EndDate = x.EndDate,
            FundingBandMaximum = x.Price
        }).ToList();

        _scenarioContext.Set(_apprenticeshipCreatedEvent);
    }

    [When(@"earnings are calculated")]
    public async Task WhenEarningsAreCalculated()
    {
        await _testContext.TestFunction.PublishEvent(_apprenticeshipCreatedEvent); 
    }

    [Given(@"earnings have been calculated")]
    public async Task GivenEarningsHaveBeenCalculated()
    {
        await _testContext.TestFunction.PublishEvent(_apprenticeshipCreatedEvent);
    }

    [When(@"care details are saved with (.*) (.*) (.*)")]
    [Given(@"care details are saved with (.*) (.*) (.*)")]
    public async Task GivenEarningsHaveBeenCalculated(bool careLeaverEmployerConsentGiven, bool isCareLeaver, bool hasEHCP)
    {
        var request = new SaveCareDetailsRequest { CareLeaverEmployerConsentGiven = careLeaverEmployerConsentGiven, IsCareLeaver = isCareLeaver, HasEHCP = hasEHCP };
        var apprenticehipKey = _apprenticeshipCreatedEvent.ApprenticeshipKey;
        await _testContext.TestInnerApi.Patch($"/apprenticeship/{apprenticehipKey}/careDetails", request);
    }

    [Then(@"recalculate event is sent with the following incentives")]
    public void ThenRecalculateEventIsSentWithTheFollowingIncentives(Table table)
    {
        var data = table.CreateSet<AdditionalPaymentExpectationModel>().ToList();
        var recalculateEvent = _testContext.MessageSession.ReceivedEvents<ApprenticeshipEarningsRecalculatedEvent>().SingleOrDefault(x => x.ApprenticeshipKey == _apprenticeshipCreatedEvent.ApprenticeshipKey);

        foreach (var expectedAdditionalPayment in data)
        {
            recalculateEvent.DeliveryPeriods.Should()
                .Contain(x => x.LearningAmount == expectedAdditionalPayment.Amount
                && x.InstalmentType == expectedAdditionalPayment.Type);
        }

    }

    private async Task<ApprenticeshipModel> GetApprenticeshipEntity()
    {
        return await _testContext.SqlDatabase.GetApprenticeship(_apprenticeshipCreatedEvent.ApprenticeshipKey);
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