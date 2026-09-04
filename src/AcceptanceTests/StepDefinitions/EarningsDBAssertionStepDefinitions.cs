using Newtonsoft.Json;
using SFA.DAS.Funding.ApprenticeshipEarnings.AcceptanceTests.Constants;
using SFA.DAS.Funding.ApprenticeshipEarnings.AcceptanceTests.Extensions;
using SFA.DAS.Funding.ApprenticeshipEarnings.AcceptanceTests.Model;
using SFA.DAS.Funding.ApprenticeshipEarnings.DataAccess.Entities;
using SFA.DAS.Funding.ApprenticeshipEarnings.DataAccess.Entities.Apprenticeship;
using SFA.DAS.Funding.ApprenticeshipEarnings.DataAccess.Entities.ShortCourse;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Models;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Models.Apprenticeship;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Models.ShortCourse;
using SFA.DAS.Funding.ApprenticeshipEarnings.Types;
using SFA.DAS.Learning.Types;
using TechTalk.SpecFlow.Assist;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.AcceptanceTests.StepDefinitions;

[Binding]
public class EarningsDBAssertionStepDefinitions
{
    private readonly ScenarioContext _scenarioContext;
    private readonly TestContext _testContext;

    public EarningsDBAssertionStepDefinitions(ScenarioContext scenarioContext, TestContext testContext)
    {
        _scenarioContext = scenarioContext;
        _testContext = testContext;
    }

    [Given(@"Earnings are generated with the correct learning amounts")]
    [Then(@"Earnings are generated with the correct learning amounts")]
    public async Task AssertEarningsLearningAmounts()
    {
        var learningKey = _scenarioContext.Get<LearningCreatedEvent>().LearningKey;
        var expectedAmount = (int)_scenarioContext[ContextKeys.ExpectedDeliveryPeriodLearningAmount];

        var updatedEntity = await _testContext.SqlDatabase.GetApprenticeshipLearning(learningKey);
        var regularInstalments = updatedEntity.Episodes.First().EarningsProfile.Instalments
            .Where(x => string.Equals(x.Type, nameof(InstalmentType.Regular), StringComparison.CurrentCultureIgnoreCase))
            .ToList();

        regularInstalments.Should().HaveCount(EventBuilderSharedDefaults.ExpectedDeliveryPeriodCount);
        regularInstalments.Should().OnlyContain(x => x.Amount == expectedAmount);
    }

    [Then(@"On programme earnings are persisted as follows")]
    [Then(@"the instalments are balanced as follows")]
    public async Task ThenOnProgrammeEarningsArePersistedAsFollows(Table table)
    {
        await AssertApprenticeshipOnProgrammeEarnings(table, _scenarioContext.Get<LearningCreatedEvent>().LearningKey);
    }

    private async Task AssertApprenticeshipOnProgrammeEarnings(Table table, Guid learningKey)
    {
        var data = table.CreateSet<EarningDbExpectationModel>().ToList();
        ApprenticeshipLearningEntity? updatedEntity;

        updatedEntity = await _testContext.SqlDatabase.GetApprenticeshipLearning(learningKey);
        var earningsInDb = updatedEntity.Episodes.First().EarningsProfile.Instalments.OrderBy(x => x.AcademicYear).ThenBy(x => x.DeliveryPeriod);

        earningsInDb.Should().HaveCount(data.Count);

        foreach (var expectedEarning in data)
        {
            earningsInDb.Should()
                .Contain(x => Math.Round(x.Amount, 2) == Math.Round(expectedEarning.Amount, 2)
                              && x.AcademicYear == expectedEarning.AcademicYear
                              && x.DeliveryPeriod == expectedEarning.DeliveryPeriod
                              && (expectedEarning.Type == null || Enum.Parse<InstalmentType>(expectedEarning.Type) == Enum.Parse<InstalmentType>(x.Type))
                    , $"Expected earning not found: {JsonConvert.SerializeObject(expectedEarning)}");
        }
    }

    [Then(@"no on programme earnings are persisted")]
    public async Task ThenNoOnProgrammeEarningsArePersisted()
    {
        var learningKeyKey = _scenarioContext.Get<LearningCreatedEvent>().LearningKey;
        var updatedEntity = await _testContext.SqlDatabase.GetApprenticeshipLearning(learningKeyKey);
        var earningsInDb = updatedEntity.Episodes.First().EarningsProfile.Instalments;

        earningsInDb.Should().BeEmpty();
    }

    [Then(@"(\d+) regular on programme earnings are persisted")]
    public async Task ThenXOnProgrammeEarningsArePersisted(int expectedEarningsCount)
    {
        var learningKeyKey = _scenarioContext.Get<LearningCreatedEvent>().LearningKey;
        var updatedEntity = await _testContext.SqlDatabase.GetApprenticeshipLearning(learningKeyKey);
        var earningsInDb = updatedEntity.Episodes.First().EarningsProfile.Instalments.Where(x => string.Equals(x.Type, nameof(InstalmentType.Regular), StringComparison.CurrentCultureIgnoreCase));

        earningsInDb.Should().HaveCount(expectedEarningsCount);
    }

    [Then(@"the total amount of on programme earnings is (.*)")]
    public async Task ThenTheTotalAmountOfOnProgrammeEarningsIs(decimal expectedTotalAmount)
    {
        var learningKey = _scenarioContext.Get<LearningCreatedEvent>().LearningKey;
        var updatedEntity = await _testContext.SqlDatabase.GetApprenticeshipLearning(learningKey);
        var earningsInDb = updatedEntity.Episodes.First().EarningsProfile.Instalments;

        decimal.Round(earningsInDb.Sum(x => x.Amount), 5).Should().Be(expectedTotalAmount);
    }
}