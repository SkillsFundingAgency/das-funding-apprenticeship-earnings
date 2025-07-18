using SFA.DAS.Learning.Types;
using SFA.DAS.Funding.ApprenticeshipEarnings.AcceptanceTests.Model;
using SFA.DAS.Funding.ApprenticeshipEarnings.Command.SaveCareDetailsCommand;
using SFA.DAS.Funding.ApprenticeshipEarnings.Command.SaveLearningSupportCommand;
using SFA.DAS.Funding.ApprenticeshipEarnings.DataAccess.Entities;
using SFA.DAS.Funding.ApprenticeshipEarnings.TestHelpers;
using SFA.DAS.Funding.ApprenticeshipEarnings.Types;
using SFA.DAS.Funding.ApprenticeshipEarnings.AcceptanceTests.Helpers;
using SFA.DAS.Funding.ApprenticeshipEarnings.Command.SaveMathsAndEnglishCommand;
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
        var expected = table.CreateSet<LearningSupportPaymentDetail>().ToList();
        await _testContext.TestInnerApi.Patch($"/apprenticeship/{_scenarioContext.Get<LearningCreatedEvent>().LearningKey}/learningSupport", expected);
    }

    [When(@"care details are saved with")]
    [Given(@"care details are saved with")]
    public async Task SaveCareDetails(Table table)
    {
        var request = table.CreateSet<SaveCareDetailsRequest>().Single();
        await _testContext.TestInnerApi.Patch($"/apprenticeship/{_scenarioContext.Get<LearningCreatedEvent>().LearningKey}/careDetails", request);
    }

    [Given(@"the following maths and english course information is provided")]
    [When(@"the following maths and english completion change request is sent")]
    public async Task GivenTheFollowingMathsAndEnglishCourseInformationIsProvided(Table table)
    {
        var expected = table.CreateSet<MathsAndEnglishDetail>().ToList();
        await _testContext.TestInnerApi.Patch($"/apprenticeship/{_scenarioContext.Get<LearningCreatedEvent>().LearningKey}/mathsAndEnglish", expected);
    }

    [When(@"the following completion is sent")]
    public async Task GivenTheFollowingCompletionInformationIsProvided(Table table)
    {
        var completionRequestModel = table.CreateSet<CompletionRequestModel>().Single();
        await _testContext.TestInnerApi.Patch($"/apprenticeship/{_scenarioContext.Get<LearningCreatedEvent>().LearningKey}/completion", completionRequestModel);
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
                && x.AdditionalPaymentType == expectedAdditionalPayment.Type);
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

        updatedEntity.Episodes.First().EarningsProfile.AdditionalPayments.Should().BeEmpty();
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
        IncentivesAssertionHelper.AssertIncentivePayment("ProviderIncentive", false, true, _scenarioContext.Get<LearningStartDateChangedEvent>(), _scenarioContext.Get<ApprenticeshipModel>());
        IncentivesAssertionHelper.AssertIncentivePayment("EmployerIncentive", false, true, _scenarioContext.Get<LearningStartDateChangedEvent>(), _scenarioContext.Get<ApprenticeshipModel>());
    }

    [Then("no first incentive payment is generated")]
    public void AssertNoFirstIncentivePayment()
    {
        IncentivesAssertionHelper.AssertIncentivePayment("ProviderIncentive", false, false, _scenarioContext.Get<LearningStartDateChangedEvent>(), _scenarioContext.Get<ApprenticeshipModel>());
        IncentivesAssertionHelper.AssertIncentivePayment("EmployerIncentive", false, false, _scenarioContext.Get<LearningStartDateChangedEvent>(), _scenarioContext.Get<ApprenticeshipModel>());
    }

    [Then("a second incentive payment is generated")]
    public void AssertSecondIncentivePayment()
    {
        IncentivesAssertionHelper.AssertIncentivePayment("ProviderIncentive", true, true, _scenarioContext.Get<LearningStartDateChangedEvent>(), _scenarioContext.Get<ApprenticeshipModel>());
        IncentivesAssertionHelper.AssertIncentivePayment("EmployerIncentive", true, true, _scenarioContext.Get<LearningStartDateChangedEvent>(), _scenarioContext.Get<ApprenticeshipModel>());
    }

    [Then("no second incentive payment is generated")]
    public void AssertNoSecondIncentivePayment()
    {
        IncentivesAssertionHelper.AssertIncentivePayment("ProviderIncentive", true, false, _scenarioContext.Get<LearningStartDateChangedEvent>(), _scenarioContext.Get<ApprenticeshipModel>());
        IncentivesAssertionHelper.AssertIncentivePayment("EmployerIncentive", true, false, _scenarioContext.Get<LearningStartDateChangedEvent>(), _scenarioContext.Get<ApprenticeshipModel>());
    }

    [Then(@"Maths and english instalments are persisted as follows")]
    public async Task ThenMathsAndEnglishInstalmentsArePersistedAsFollows(Table table)
    {
        var data = table.CreateSet<MathsAndEnglishInstalmentDbExpectationModel>().ToList();

        var learningCreatedEvent = _scenarioContext.Get<LearningCreatedEvent>();

        var updatedEntity = await _testContext.SqlDatabase.GetApprenticeship(learningCreatedEvent.LearningKey);

        var mathsAndEnglishCoursesInDb = updatedEntity.Episodes.First().EarningsProfile.MathsAndEnglishCourses;

        foreach (var expectedInstalment in data)
        {
            var courseInDb = mathsAndEnglishCoursesInDb.SingleOrDefault(x => x.Course.TrimEnd() == expectedInstalment.Course);
            courseInDb.Should().NotBeNull();

            courseInDb.Instalments.Should()
                .Contain(x => x.Amount == expectedInstalment.Amount
                              && x.AcademicYear == expectedInstalment.AcademicYear
                              && x.DeliveryPeriod == expectedInstalment.DeliveryPeriod);
        }
    }

    
}


