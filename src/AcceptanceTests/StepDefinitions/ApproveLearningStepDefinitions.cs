using SFA.DAS.Funding.ApprenticeshipEarnings.Types;
using SFA.DAS.Learning.Types;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.AcceptanceTests.StepDefinitions;

[Binding]
public class ApproveLearningStepDefinitions
{
    private readonly ScenarioContext _scenarioContext;
    private readonly TestContext _testContext;

    public ApproveLearningStepDefinitions(ScenarioContext scenarioContext, TestContext testContext)
    {
        _scenarioContext = scenarioContext;
        _testContext = testContext;
    }

    [When("a LearningApproved event is received for the short course")]
    public async Task WhenLearningApprovedEventReceived()
    {
        var request = _scenarioContext.Get<CreateUnapprovedShortCourseLearningRequest>();
        await _testContext.TestFunction.PublishEvent(new LearningApprovedEvent { LearningKey = request.LearningKey });
    }

    [Given(@"the short course earnings profile is not yet approved")]
    public async Task GivenTheShortCourseEarningsProfileIsNotYetApproved()
    {
        var request = _scenarioContext.Get<CreateUnapprovedShortCourseLearningRequest>();
        var entity = await _testContext.SqlDatabase.GetShortCourseLearning(request.LearningKey);
        entity!.Episodes.First().EarningsProfile.IsApproved.Should().BeFalse();
    }


    [Then("the short course earnings profile is marked as approved")]
    public async Task ThenShortCourseEarningsProfileIsApproved()
    {
        var request = _scenarioContext.Get<CreateUnapprovedShortCourseLearningRequest>();
        var entity = await _testContext.SqlDatabase.GetShortCourseLearning(request.LearningKey);
        entity!.Episodes.First().EarningsProfile.IsApproved.Should().BeTrue();
    }
}
