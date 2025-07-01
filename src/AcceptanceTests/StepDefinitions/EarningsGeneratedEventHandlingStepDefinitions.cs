using SFA.DAS.Apprenticeships.Types;
using SFA.DAS.Funding.ApprenticeshipEarnings.AcceptanceTests.Constants;
using SFA.DAS.Funding.ApprenticeshipEarnings.AcceptanceTests.Extensions;
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

    [Given(@"Earnings are generated with the correct learning amounts")]
    [Then(@"Earnings are generated with the correct learning amounts")]
    public async Task AssertEarningsGeneratedEvent()
    {
        await WaitHelper.WaitForIt(() => _testContext.MessageSession.ReceivedEvents<EarningsGeneratedEvent>().Any(x => x.EventMatchesExpectation(_scenarioContext.Get<ApprenticeshipCreatedEvent>().Uln, (int)_scenarioContext[ContextKeys.ExpectedDeliveryPeriodLearningAmount])), "Failed to find published EarningsGenerated event");
    }

    [Then(@"Earnings are not generated for that apprenticeship")]
    public async Task AssertNoEarningsGeneratedEvent()
    {
        await WaitHelper.WaitForUnexpected(() => _testContext.MessageSession.ReceivedEvents<EarningsGeneratedEvent>().Any(x => x.Uln == _scenarioContext.Get<ApprenticeshipCreatedEvent>().Uln), "Found published EarningsGenerated event when expecting no earnings to be generated", TimeSpan.FromSeconds(10));
    }

    [Then(@"the funding line type 16-18 must be used in the calculation")]
    public async Task ThenThe16To18FundingLineTypeIsUsed()
    {
        await WaitHelper.WaitForIt(() => _testContext.MessageSession.ReceivedEvents<EarningsGeneratedEvent>().Any(x => x.EventMatchesExpectation(_scenarioContext.Get<ApprenticeshipCreatedEvent>().Uln, "16-18 Apprenticeship (Employer on App Service)")), "Failed to find published EarningsGenerated event");
    }

    [Then(@"the funding line type 19 plus must be used in the calculation")]
    public async Task ThenThe19AndOverFundingLineTypeIsUsed()
    {
        await WaitHelper.WaitForIt(() => _testContext.MessageSession.ReceivedEvents<EarningsGeneratedEvent>().Any(x => x.EventMatchesExpectation(_scenarioContext.Get<ApprenticeshipCreatedEvent>().Uln, "19+ Apprenticeship (Employer on App Service)")), "Failed to find published EarningsGenerated event");
    }

    [Then(@"On programme earnings are persisted as follows")]
    public async Task ThenOnProgrammeEarningsArePersistedAsFollows(Table table)
    {
        var data = table.CreateSet<EarningDbExpectationModel>().ToList();

        var apprenticeshipKey = _scenarioContext.Get<ApprenticeshipCreatedEvent>().ApprenticeshipKey;

        var updatedEntity = await _testContext.SqlDatabase.GetApprenticeship(apprenticeshipKey);
        var queryEarningsDbRecords = await _testContext.SqlDatabase.GetQueryEarnings(apprenticeshipKey);


        var earningsInDb = updatedEntity.Episodes.First().EarningsProfile.Instalments;

        earningsInDb.Should().HaveCount(data.Count);
        queryEarningsDbRecords.Should().HaveCount(data.Count);


        foreach (var expectedEarning in data)
        {
            earningsInDb.Should()
                .Contain(x => x.Amount == expectedEarning.Amount
                              && x.AcademicYear == expectedEarning.AcademicYear
                              && x.DeliveryPeriod == expectedEarning.DeliveryPeriod);

            queryEarningsDbRecords.Should()
                .Contain(x => x.Amount == expectedEarning.Amount
                              && x.AcademicYear == expectedEarning.AcademicYear
                              && x.DeliveryPeriod == expectedEarning.DeliveryPeriod);
        }
    }
}