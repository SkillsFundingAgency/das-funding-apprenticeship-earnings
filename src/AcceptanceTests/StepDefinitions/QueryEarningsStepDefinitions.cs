using SFA.DAS.Apprenticeships.Types;
using SFA.DAS.Funding.ApprenticeshipEarnings.AcceptanceTests.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TechTalk.SpecFlow.Assist;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.AcceptanceTests.StepDefinitions;

/// <summary>
/// This relates to the query earnings table which is used for reporting purposes.
/// </summary>
[Binding]
public class QueryEarningsStepDefinitions
{
    private readonly ScenarioContext _scenarioContext;
    private readonly TestContext _testContext;

    public QueryEarningsStepDefinitions(ScenarioContext scenarioContext, TestContext testContext)
    {
        _scenarioContext = scenarioContext;
        _testContext = testContext;
    }

    [Then(@"the following Query Earnings records are created")]
    public async Task GivenTheFollowingPriceEpisodes(Table table)
    {
        var queryEarnings = table.CreateSet<EarningDbExpectationModel>().ToList();

        var apprenticeshipCreatedEvent = _scenarioContext.Get<ApprenticeshipCreatedEvent>();
        var apprenticeshipKey = apprenticeshipCreatedEvent.ApprenticeshipKey;
        var records = await _testContext.SqlDatabase.GetQueryEarnings(apprenticeshipKey);

        records.Should().HaveCount(queryEarnings.Count, "The number of query earnings records does not match the expected count.");
        
        foreach (var expectedQueryEarning in queryEarnings)
        {
            records.Should()
                .Contain(x => x.Amount == expectedQueryEarning.Amount
                              && x.AcademicYear == expectedQueryEarning.AcademicYear
                              && x.DeliveryPeriod == expectedQueryEarning.DeliveryPeriod);
        }
    }
}
