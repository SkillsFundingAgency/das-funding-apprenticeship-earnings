using SFA.DAS.Funding.ApprenticeshipEarnings.AcceptanceTests.Helpers;
using SFA.DAS.Funding.ApprenticeshipEarnings.AcceptanceTests.Model;
using SFA.DAS.Funding.ApprenticeshipEarnings.Command.SaveCareDetailsCommand;
using SFA.DAS.Funding.ApprenticeshipEarnings.Command.UpdateLearningSupportCommand;
using SFA.DAS.Funding.ApprenticeshipEarnings.Command.UpdateEnglishAndMathsCommand;
using SFA.DAS.Funding.ApprenticeshipEarnings.Command.UpdateOnProgrammeCommand;
using SFA.DAS.Funding.ApprenticeshipEarnings.DataAccess.Entities;
using SFA.DAS.Funding.ApprenticeshipEarnings.TestHelpers;
using SFA.DAS.Funding.ApprenticeshipEarnings.Types;
using SFA.DAS.Learning.Types;
using TechTalk.SpecFlow.Assist;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.AcceptanceTests.StepDefinitions;

[Binding]
public class AdditionalPaymentsStepDefinitions
{
    private readonly ScenarioContext _scenarioContext;
    private readonly TestContext _testContext;

    public AdditionalPaymentsStepDefinitions(ScenarioContext scenarioContext, TestContext testContext)
    {
        _scenarioContext = scenarioContext;
        _testContext = testContext;
    }

    [Given(@"the following learning support payment information is provided")]
    public async Task GivenTheFollowingLearningSupportPaymentInformationIsProvided(Table table)
    {
        var learningSupportItems = table.CreateSet<LearningSupportItem>().ToList();
        var request = new UpdateLearningSupportRequest
        {
            LearningSupport = learningSupportItems
        };
        await _testContext.TestInnerApi.Put($"/learning/{_scenarioContext.Get<LearningCreatedEvent>().LearningKey}/learning-support", request);
    }

    [When(@"care details are saved with")]
    [Given(@"care details are saved with")]
    public async Task SaveCareDetails(Table table)
    {
        var request = table.CreateSet<SaveCareDetailsRequest>().Single();
        await _testContext.TestInnerApi.Patch($"/learning/{_scenarioContext.Get<LearningCreatedEvent>().LearningKey}/careDetails", request);
    }

    [Then(@"recalculate event is sent with the following incentives")]
    public void ThenRecalculateEventIsSentWithTheFollowingIncentives(Table table)
    {
        var data = table.CreateSet<AdditionalPaymentExpectationModel>().ToList();
        var learningCreatedEvent = _scenarioContext.Get<LearningCreatedEvent>();
        var recalculateEvent = _testContext.MessageSession.ReceivedEvents<ApprenticeshipEarningsRecalculatedEvent>().SingleOrDefault(x => x.ApprenticeshipKey == learningCreatedEvent.LearningKey);

        foreach (var expectedAdditionalPayment in data)
        {
            recalculateEvent.DeliveryPeriods.Should()
                .Contain(x => x.LearningAmount == expectedAdditionalPayment.Amount
                && x.InstalmentType == expectedAdditionalPayment.Type);
        }

    }

    [Given(@"Additional Payments are persisted as follows")]
    [Then(@"Additional Payments are persisted as follows")]
    public async Task ThenAdditionalPaymentsArePersistedAsFollows(Table table)
    {
        var data = table.CreateSet<AdditionalPaymentDbExpectationModel>().ToList();

        var learningCreatedEvent = _scenarioContext.Get<LearningCreatedEvent>();

        var updatedEntity = await _testContext.SqlDatabase.GetApprenticeship(learningCreatedEvent.LearningKey);

        var additionalPaymentsInDb = updatedEntity.Episodes.First().EarningsProfile.AdditionalPayments;

        additionalPaymentsInDb.Should().HaveCount(data.Count);

        foreach (var expectedAdditionalPayment in data)
        {
            additionalPaymentsInDb.Should()
                .Contain(x => x.Amount == expectedAdditionalPayment.Amount
                && x.DueDate == expectedAdditionalPayment.DueDate
                && x.AdditionalPaymentType == expectedAdditionalPayment.Type
                && x.IsAfterLearningEnded == expectedAdditionalPayment.IsAfterLearningEnded);
        }
    }

    [Then(@"an EarningsGeneratedEvent is raised with the following incentives as Delivery Periods")]
    public async Task ThenAdditionalPaymentsAreGeneratedWithTheFollowingIncentivesAsDeliveryPeriods(Table table)
    {
        await WaitHelper.WaitForIt(() =>
                _testContext.MessageSession.ReceivedEvents<EarningsGeneratedEvent>().Any(),
            "Failed to find any EarningsGeneratedEvent"
        );

        var expected = table.CreateSet<AdditionalPaymentExpectationModel>()
            .Select(e => new
            {
                e.Type,
                e.Amount,
                e.CalendarMonth,
                e.CalendarYear
            }).ToList();

        var allActualIncentives = _testContext.MessageSession.ReceivedEvents<EarningsGeneratedEvent>()
            .SelectMany(e => e.DeliveryPeriods)
            .Where(x => x.InstalmentType is "ProviderIncentive" or "EmployerIncentive")
            .Select(dp => new
            {
                Type = dp.InstalmentType,
                Amount = dp.LearningAmount,
                dp.CalendarMonth,
                CalendarYear = dp.CalenderYear
            }).ToList();

        allActualIncentives.Should().BeEquivalentTo(expected, options => options.IncludingFields());
    }

    [Then(@"no Additional Payments are persisted")]
    public async Task ThenNoAdditionalPaymentsArePersisted()
    {
        var learningCreatedEvent = _scenarioContext.Get<LearningCreatedEvent>();

        var updatedEntity = await _testContext.SqlDatabase.GetApprenticeship(learningCreatedEvent.LearningKey);

        updatedEntity.Episodes.First().EarningsProfile.AdditionalPayments.Where(x => !x.IsAfterLearningEnded).Should().BeEmpty();
    }

    [Then(@"an EarningsGeneratedEvent is raised with no incentives as Delivery Periods")]
    public async Task ThenAnEarningsGeneratedEventIsRaisedWithNoIncentivesAsDeliveryPeriods()
    {
        await WaitHelper.WaitForIt(() =>
                _testContext.MessageSession.ReceivedEvents<EarningsGeneratedEvent>().Any(),
            "Failed to find any EarningsGeneratedEvent"
        );

        var allActualIncentives = _testContext.MessageSession.ReceivedEvents<EarningsGeneratedEvent>()
            .SelectMany(e => e.DeliveryPeriods)
            .Where(x => x.InstalmentType is "ProviderIncentive" or "EmployerIncentive")
            .ToList();

        allActualIncentives.Should().BeEmpty();
    }

    [Then("a first incentive payment is generated")]
    public void AssertFirstIncentivePayment()
    {
        var updateOnProgrammeRequest = _scenarioContext.Get<UpdateOnProgrammeRequest>();
        var apprenticeshipModel = _scenarioContext.Get<ApprenticeshipModel>();
        IncentivesAssertionHelper.AssertIncentivePayment("ProviderIncentive", false, true, updateOnProgrammeRequest, apprenticeshipModel);
        IncentivesAssertionHelper.AssertIncentivePayment("EmployerIncentive", false, true, updateOnProgrammeRequest, apprenticeshipModel);
    }

    [Then("no first incentive payment is generated")]
    public void AssertNoFirstIncentivePayment()
    {
        var updateOnProgrammeRequest = _scenarioContext.Get<UpdateOnProgrammeRequest>();
        var apprenticeshipModel = _scenarioContext.Get<ApprenticeshipModel>();
        IncentivesAssertionHelper.AssertIncentivePayment("ProviderIncentive", false, false, updateOnProgrammeRequest, apprenticeshipModel);
        IncentivesAssertionHelper.AssertIncentivePayment("EmployerIncentive", false, false, updateOnProgrammeRequest, apprenticeshipModel);
    }

    [Then("a second incentive payment is generated")]
    public void AssertSecondIncentivePayment()
    {
        var updateOnProgrammeRequest = _scenarioContext.Get<UpdateOnProgrammeRequest>();
        var apprenticeshipModel = _scenarioContext.Get<ApprenticeshipModel>();
        IncentivesAssertionHelper.AssertIncentivePayment("ProviderIncentive", true, true, updateOnProgrammeRequest, apprenticeshipModel);
        IncentivesAssertionHelper.AssertIncentivePayment("EmployerIncentive", true, true, updateOnProgrammeRequest, apprenticeshipModel);
    }

    [Then("no second incentive payment is generated")]
    public void AssertNoSecondIncentivePayment()
    {
        var updateOnProgrammeRequest = _scenarioContext.Get<UpdateOnProgrammeRequest>();
        var apprenticeshipModel = _scenarioContext.Get<ApprenticeshipModel>();
        IncentivesAssertionHelper.AssertIncentivePayment("ProviderIncentive", true, false, updateOnProgrammeRequest, apprenticeshipModel);
        IncentivesAssertionHelper.AssertIncentivePayment("EmployerIncentive", true, false, updateOnProgrammeRequest, apprenticeshipModel);
    }

}


