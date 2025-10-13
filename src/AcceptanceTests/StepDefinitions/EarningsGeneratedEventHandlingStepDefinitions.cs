using Newtonsoft.Json;
using SFA.DAS.Learning.Types;
using SFA.DAS.Funding.ApprenticeshipEarnings.AcceptanceTests.Constants;
using SFA.DAS.Funding.ApprenticeshipEarnings.AcceptanceTests.Extensions;
using SFA.DAS.Funding.ApprenticeshipEarnings.AcceptanceTests.Model;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Apprenticeship;
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
        await WaitHelper.WaitForIt(() => _testContext.MessageSession.ReceivedEvents<EarningsGeneratedEvent>().Any(x => x.EventMatchesExpectation(_scenarioContext.Get<LearningCreatedEvent>().Uln, (int)_scenarioContext[ContextKeys.ExpectedDeliveryPeriodLearningAmount])), "Failed to find published EarningsGenerated event");
    }

    [Then(@"Earnings are not generated for that apprenticeship")]
    public async Task AssertNoEarningsGeneratedEvent()
    {
        await WaitHelper.WaitForUnexpected(() => _testContext.MessageSession.ReceivedEvents<EarningsGeneratedEvent>().Any(x => x.Uln == _scenarioContext.Get<LearningCreatedEvent>().Uln), "Found published EarningsGenerated event when expecting no earnings to be generated", TimeSpan.FromSeconds(10));
    }

    [Then(@"the funding line type 16-18 must be used in the calculation")]
    public async Task ThenThe16To18FundingLineTypeIsUsed()
    {
        await WaitHelper.WaitForIt(() => _testContext.MessageSession.ReceivedEvents<EarningsGeneratedEvent>().Any(x => x.EventMatchesExpectation(_scenarioContext.Get<LearningCreatedEvent>().Uln, "16-18 Apprenticeship (Employer on App Service)")), "Failed to find published EarningsGenerated event");
    }

    [Then(@"the funding line type 19 plus must be used in the calculation")]
    public async Task ThenThe19AndOverFundingLineTypeIsUsed()
    {
        await WaitHelper.WaitForIt(() => _testContext.MessageSession.ReceivedEvents<EarningsGeneratedEvent>().Any(x => x.EventMatchesExpectation(_scenarioContext.Get<LearningCreatedEvent>().Uln, "19+ Apprenticeship (Employer on App Service)")), "Failed to find published EarningsGenerated event");
    }

    [Then(@"On programme earnings are persisted as follows")]
    [Then(@"the instalments are balanced as follows")]
    public async Task ThenOnProgrammeEarningsArePersistedAsFollows(Table table)
    {
        var data = table.CreateSet<EarningDbExpectationModel>().ToList();
        var learningKeyKey = _scenarioContext.Get<LearningCreatedEvent>().LearningKey;
        var updatedEntity = await _testContext.SqlDatabase.GetApprenticeship(learningKeyKey);
        var queryEarningsDbRecords = await _testContext.SqlDatabase.GetQueryEarnings(learningKeyKey);
        var earningsInDb = updatedEntity.Episodes.First().EarningsProfile.Instalments.Where(x => !x.IsAfterLearningEnded);

        earningsInDb.Should().HaveCount(data.Count);
        queryEarningsDbRecords.Should().HaveCount(data.Count);

        foreach (var expectedEarning in data)
        {
            earningsInDb.Should()
                .Contain(x => x.Amount == expectedEarning.Amount
                              && x.AcademicYear == expectedEarning.AcademicYear
                              && x.DeliveryPeriod == expectedEarning.DeliveryPeriod
                              && (expectedEarning.Type == null || Enum.Parse<InstalmentType>(expectedEarning.Type) == Enum.Parse<InstalmentType>(x.Type))
                , $"Expected earning not found: {JsonConvert.SerializeObject(expectedEarning)}");

            queryEarningsDbRecords.Should()
                .Contain(x => x.Amount == expectedEarning.Amount
                              && x.AcademicYear == expectedEarning.AcademicYear
                              && x.DeliveryPeriod == expectedEarning.DeliveryPeriod
                    , $"Expected earning not found: {JsonConvert.SerializeObject(expectedEarning)}");
        }
    }

    [Then(@"no on programme earnings are persisted")]
    public async Task ThenNoOnProgrammeEarningsArePersisted()
    {
        var learningKeyKey = _scenarioContext.Get<LearningCreatedEvent>().LearningKey;
        var updatedEntity = await _testContext.SqlDatabase.GetApprenticeship(learningKeyKey);
        var queryEarningsDbRecords = await _testContext.SqlDatabase.GetQueryEarnings(learningKeyKey);
        var earningsInDb = updatedEntity.Episodes.First().EarningsProfile.Instalments.Where(x => !x.IsAfterLearningEnded);

        earningsInDb.Should().BeEmpty();
        queryEarningsDbRecords.Should().BeEmpty();
    }
}