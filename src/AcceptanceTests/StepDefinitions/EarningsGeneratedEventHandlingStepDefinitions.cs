using NServiceBus;
using NUnit.Framework;
using SFA.DAS.Apprenticeships.Types;
using SFA.DAS.Funding.ApprenticeshipEarnings.AcceptanceTests.Helpers;
using SFA.DAS.Funding.ApprenticeshipEarnings.AcceptanceTests.Model;
using SFA.DAS.Funding.ApprenticeshipEarnings.TestHelpers;
using SFA.DAS.Funding.ApprenticeshipEarnings.Types;
using TechTalk.SpecFlow.Assist;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.AcceptanceTests.StepDefinitions;

[Binding]
public class EarningsGeneratedEventHandlingStepDefinitions
{
    private readonly ScenarioContext _scenarioContext;
    private readonly TestContext _testContext;

    public EarningsGeneratedEventHandlingStepDefinitions(ScenarioContext scenarioContext, TestContext testContext)
    {
        _scenarioContext = scenarioContext;
        _testContext = testContext;
    }

    [Then(@"Earnings are generated with the correct learning amounts")]
    public async Task AssertEarningsGeneratedEvent()
    {
        await WaitHelper.WaitForIt(() => _testContext.MessageSession.ReceivedEvents<EarningsGeneratedEvent>().Any(EventMatchesExpectation), "Failed to find published EarningsGenerated event");
    }

    [Then(@"Earnings are not generated for that apprenticeship")]
    public async Task AssertNoEarningsGeneratedEvent()
    {
        await WaitHelper.WaitForUnexpected(() => _testContext.MessageSession.ReceivedEvents<EarningsGeneratedEvent>().Any(x => x.Uln == _scenarioContext[ContextKeys.ExpectedUln].ToString()), "Found published EarningsGenerated event when expecting no earnings to be generated", TimeSpan.FromSeconds(10));
    }

    [Then(@"the funding line type 16-18 must be used in the calculation")]
    public async Task ThenThe16To18FundingLineTypeIsUsed()
    {
        await WaitHelper.WaitForIt(() => _testContext.MessageSession.ReceivedEvents<EarningsGeneratedEvent>().Any(x => EventMatchesExpectation(x, "16-18 Apprenticeship (Employer on App Service)")), "Failed to find published EarningsGenerated event");
    }

    [Then(@"the funding line type 19 plus must be used in the calculation")]
    public async Task ThenThe19AndOverFundingLineTypeIsUsed()
    {
        await WaitHelper.WaitForIt(() => _testContext.MessageSession.ReceivedEvents<EarningsGeneratedEvent>().Any(x => EventMatchesExpectation(x, "19+ Apprenticeship (Employer on App Service)")), "Failed to find published EarningsGenerated event");
    }

    private bool EventMatchesExpectation(EarningsGeneratedEvent earningsGeneratedEvent)
    {
        return earningsGeneratedEvent.DeliveryPeriods.Count == (int)_scenarioContext[ContextKeys.ExpectedDeliveryPeriodCount]
            && earningsGeneratedEvent.DeliveryPeriods.All(x => x.LearningAmount == (int)_scenarioContext[ContextKeys.ExpectedDeliveryPeriodLearningAmount]
            && earningsGeneratedEvent.Uln == _scenarioContext[ContextKeys.ExpectedUln].ToString());
    }

    private bool EventMatchesExpectation(EarningsGeneratedEvent earningsGeneratedEvent, string expectedFundingLineType)
    {
        return earningsGeneratedEvent.DeliveryPeriods.All(y => y.FundingLineType == expectedFundingLineType) &&
               earningsGeneratedEvent.Uln == _scenarioContext[ContextKeys.ExpectedUln].ToString() &&
               earningsGeneratedEvent.EarningsProfileId != Guid.Empty;
    }

    [Then(@"Additional Payments are persisted as follows")]
    public async Task ThenAdditionalPaymentsArePersistedAsFollows(Table table)
    {
        var data = table.CreateSet<AdditionalPaymentDbExpectationModel>().ToList();

        var apprenticeshipCreatedEvent = _scenarioContext.Get<ApprenticeshipCreatedEvent>();

        var updatedEntity = await _testContext.SqlDatabase.GetApprenticeship(apprenticeshipCreatedEvent.ApprenticeshipKey);

        foreach (var expectedAdditionalPayment in data)
        {
            updatedEntity.Episodes.First()
                .EarningsProfile.AdditionalPayments.Should()
                .Contain(x => x.Amount == expectedAdditionalPayment.Amount
                && x.DueDate == expectedAdditionalPayment.DueDate
                && x.AdditionalPaymentType == expectedAdditionalPayment.Type);
        }
    }

    [Then(@"an EarningsGeneratedEvent is raised with the following Delivery Periods")]
    public async Task ThenAdditionalPaymentsAreGeneratedWithTheFollowingInformation(Table table)
    {
        var data = table.CreateSet<AdditionalPaymentExpectationModel>().ToList();
        var tasks = data.Select(expectation =>
            WaitHelper.WaitForIt(() =>
                    _testContext.MessageSession.ReceivedEvents<EarningsGeneratedEvent>().Any(x => HasIncentivePayment(x, expectation)),
                "Failed to find published Additional Payment"
            )
        ).ToList();

        await Task.WhenAll(tasks);
    }

    private bool HasIncentivePayment(EarningsGeneratedEvent earningsGeneratedEvent, AdditionalPaymentExpectationModel expected)
    {
        return earningsGeneratedEvent.DeliveryPeriods.Any(x =>
            x.InstalmentType == expected.Type &&
            x.LearningAmount == expected.Amount &&
            x.CalendarMonth == expected.CalendarMonth &&
            x.CalenderYear == expected.CalendarYear);
    }
}