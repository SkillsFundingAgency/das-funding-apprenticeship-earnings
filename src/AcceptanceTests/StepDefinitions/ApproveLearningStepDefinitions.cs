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

    [Given("a LearningApproved event is received for the short course")]
    [When("a LearningApproved event is received for the short course")]
    [Given("the Short Course is approved by the Employer")]
    [When("the Short Course is approved by the Employer")]
    public async Task WhenLearningApprovedEventReceived()
    {
        var request = _scenarioContext.Get<CreateUnapprovedShortCourseLearningRequest>();
        var learningApprovedEvent = new LearningApprovedEvent { LearningKey = request.LearningKey, EpisodeKey = request.EpisodeKey, EmployerAccountId = 112, FundingAccountId = 114, LearnerKey = Guid.NewGuid(), LearnerRef = "Ref493" };
        _scenarioContext.Set(learningApprovedEvent);
        await _testContext.TestFunction.PublishEvent(learningApprovedEvent);
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

    [Then("the short course earnings profile for the current episode is marked as approved")]
    public async Task ThenShortCourseEarningsProfileForCurrentEpisodeIsApproved()
    {
        var request = _scenarioContext.Get<CreateUnapprovedShortCourseLearningRequest>();
        var entity = await _testContext.SqlDatabase.GetShortCourseLearning(request.LearningKey);
        entity!.Episodes.Single(e => e.Key == request.EpisodeKey).EarningsProfile.IsApproved.Should().BeTrue();
    }
}
