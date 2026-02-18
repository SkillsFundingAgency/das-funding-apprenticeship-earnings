using System.Text.Json;
using NUnit.Framework;
using SFA.DAS.Funding.ApprenticeshipEarnings.AcceptanceTests.Model;
using SFA.DAS.Funding.ApprenticeshipEarnings.DataAccess.Entities;
using SFA.DAS.Funding.ApprenticeshipEarnings.TestHelpers;
using SFA.DAS.Funding.ApprenticeshipEarnings.Types;
using TechTalk.SpecFlow.Assist;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.AcceptanceTests.StepDefinitions;

[Binding]
public class ShortCourseStepDefinitions
{
    private readonly ScenarioContext _scenarioContext;
    private readonly TestContext _testContext;

    public ShortCourseStepDefinitions(ScenarioContext scenarioContext, TestContext testContext)
    {
        _scenarioContext = scenarioContext;
        _testContext = testContext;
    }

    [Given("a short course has been created with the following information")]
    public async Task CreateUnapprovedShortCourseLearning(Table table)
    {
        _scenarioContext.TryGetValue<CreateUnapprovedShortCourseLearningRequest>(out var existingRequest);

        var request = table
            .CreateInstance<UnapprovedShortCourseSetupModel>()
            .ToApiRequest(existingRequest?.LearningKey);

        _scenarioContext.Set(request);

        await _testContext.TestInnerApi.Post($"/shortCourses", request);
    }

    [Then("Calculation Data is serialised")]
    public async Task CalculationDataIsSerialised()
    {
        var request = _scenarioContext.Get<CreateUnapprovedShortCourseLearningRequest>();
        var updatedEntity = await _testContext.SqlDatabase.GetApprenticeship(request.LearningKey);

        JsonSerializer
            .Deserialize<CreateUnapprovedShortCourseLearningRequest>(updatedEntity.Episodes.First().EarningsProfile.CalculationData)
            .Should().BeEquivalentTo(request);
    }

    [Then("the short course earnings history is maintained")]
    public async Task AssertHistoryUpdated()
    {
        var request = _scenarioContext.Get<CreateUnapprovedShortCourseLearningRequest>();
        var updatedEntity = await _testContext.SqlDatabase.GetApprenticeship(request.LearningKey);
        var currentEpisode = updatedEntity!.GetCurrentEpisode(TestSystemClock.Instance());

        var history = await _testContext.SqlDatabase.GetHistory(currentEpisode.EarningsProfile.EarningsProfileId);

        if (history.Count == 0)
        {
            Assert.Fail("No earning history created");
        }

        history.First().Version.Should().Be(currentEpisode.EarningsProfile.Version);
    }
}