using System.Text.Json;
using SFA.DAS.Funding.ApprenticeshipEarnings.AcceptanceTests.Model;
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
        var request = table
            .CreateInstance<UnapprovedShortCourseSetupModel>()
            .ToApiRequest();

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
}