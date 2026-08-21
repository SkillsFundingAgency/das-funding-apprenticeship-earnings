using SFA.DAS.Funding.ApprenticeshipEarnings.AcceptanceTests.Extensions;
using SFA.DAS.Funding.ApprenticeshipEarnings.AcceptanceTests.Model;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Extensions;
using SFA.DAS.Funding.ApprenticeshipEarnings.Types;
using SFA.DAS.Learning.Types;
using TechTalk.SpecFlow.Assist;

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

    [Given("an apprenticeship has been created as a draft with the following information")]
    public async Task GivenAnApprenticeshipHasBeenCreatedAsADraft(Table table)
    {
        var request = table.CreateInstance<UnapprovedApprenticeshipSetupModel>().ToApiRequest();

        _scenarioContext.Set(request);

        await _testContext.TestInnerApi.Post("/learning", request);
    }

    [Given("the apprenticeship earnings profile is not yet approved")]
    public async Task GivenTheApprenticeshipEarningsProfileIsNotYetApproved()
    {
        var request = _scenarioContext.Get<CreateUnapprovedApprenticeshipLearningRequest>();
        var entity = await _testContext.SqlDatabase.GetApprenticeshipLearning(request.LearningKey);
        entity!.Episodes.Single(x => x.Key == request.EpisodeKey).EarningsProfile.IsApproved.Should().BeFalse();
    }

    [Given("a LearningApproved event is received for the apprenticeship")]
    [When("a LearningApproved event is received for the apprenticeship")]
    public async Task WhenLearningApprovedEventReceivedForApprenticeship()
    {
        var request = _scenarioContext.Get<CreateUnapprovedApprenticeshipLearningRequest>();
        var learningApprovedEvent = new LearningApprovedEvent
        {
            LearningKey = request.LearningKey,
            EpisodeKey = request.EpisodeKey,
            ApprovalsApprenticeshipId = _scenarioContext.GetApprovalsApprenticeshipId(),
            EmployerAccountId = _scenarioContext.GetEmployerAccountId(),
            FundingAccountId = _scenarioContext.GetFundingAccountId(),
            LearnerKey = _scenarioContext.GetLearnerKey(),
            LearnerRef = _scenarioContext.GetLearnerRef(),
            EmployerType = _scenarioContext.GetEmployerType()
        };
        _scenarioContext.Set(learningApprovedEvent);
        await _testContext.TestFunction.PublishEvent(learningApprovedEvent);
    }

    [Then("the apprenticeship earnings profile is marked as approved")]
    public async Task ThenApprenticeshipEarningsProfileIsApproved()
    {
        var request = _scenarioContext.Get<CreateUnapprovedApprenticeshipLearningRequest>();
        var entity = await _testContext.SqlDatabase.GetApprenticeshipLearning(request.LearningKey);
        var episode = entity!.Episodes.Single(x => x.Key == request.EpisodeKey);

        episode.EarningsProfile.IsApproved.Should().BeTrue();
        episode.EmployerAccountId.Should().Be(_scenarioContext.GetEmployerAccountId());
        episode.FundingEmployerAccountId.Should().Be(_scenarioContext.GetFundingAccountId());
        episode.EmployerType.Should().Be(_scenarioContext.GetEmployerType().ToEmployerType());
        entity.ApprovalsApprenticeshipId.Should().Be(_scenarioContext.GetApprovalsApprenticeshipId());
    }

    [Given("a LearningApproved event is received for the short course")]
    [When("a LearningApproved event is received for the short course")]
    [Given("the Short Course is approved by the Employer")]
    [When("the Short Course is approved by the Employer")]
    public async Task WhenLearningApprovedEventReceived()
    {
        var request = _scenarioContext.Get<CreateUnapprovedShortCourseLearningRequest>();
        var learningApprovedEvent = new LearningApprovedEvent { 
            LearningKey = request.LearningKey, 
            EpisodeKey = request.EpisodeKey, 
            ApprovalsApprenticeshipId = _scenarioContext.GetApprovalsApprenticeshipId(),
            EmployerAccountId = _scenarioContext.GetEmployerAccountId(), 
            FundingAccountId = _scenarioContext.GetFundingAccountId(), 
            LearnerKey = _scenarioContext.GetLearnerKey(),
            LearnerRef = _scenarioContext.GetLearnerRef(),
            EmployerType = _scenarioContext.GetEmployerType()
        };
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
