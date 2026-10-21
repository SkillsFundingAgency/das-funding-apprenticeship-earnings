using FluentAssertions;
using SFA.DAS.Funding.ApprenticeshipEarnings.Types;
using SFA.DAS.Payments.EarningEvents.Messages.External;
using System.Linq;
using System.Threading.Tasks;
using TechTalk.SpecFlow;
using LearningType = SFA.DAS.Payments.EarningEvents.Messages.External.LearningType;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.AcceptanceTests.StepDefinitions;

[Binding]
public class FurtherApprovalStepDefinitions
{
    private readonly ScenarioContext _scenarioContext;
    private readonly TestContext _testContext;

    public FurtherApprovalStepDefinitions(ScenarioContext scenarioContext, TestContext testContext)
    {
        _scenarioContext = scenarioContext;
        _testContext = testContext;
    }

    [Then(@"the episode is stored as (needing|not needing) further approval")]
    public async Task ThenTheEpisodeIsStoredAsNeedingFurtherApproval(string needing)
    {
        var request = _scenarioContext.Get<CreateUnapprovedApprenticeshipLearningRequest>();
        var dbEntity = await _testContext.SqlDatabase.GetApprenticeshipLearning(request.LearningKey);

        var episode = dbEntity!.Episodes.Single(x => x.Key == request.EpisodeKey);
        episode.FurtherApprovalNeeded.Should().Be(needing == "needing");
    }

    [Then(@"(\d+) withhold payments events? (?:is|are) published")]
    public void ThenWithholdPaymentsEventsArePublished(int expectedCount)
    {
        _testContext.MessageSession.ReceivedEvents<WithholdPaymentsForLearningEvent>().Should().HaveCount(expectedCount);
    }

    [Then("the withhold payments event is published with the correct information for the apprenticeship")]
    public void ThenTheWithholdPaymentsEventIsPublishedWithTheCorrectInformation()
    {
        var request = _scenarioContext.Get<CreateUnapprovedApprenticeshipLearningRequest>();

        var @event = _testContext.MessageSession.ReceivedEvents<WithholdPaymentsForLearningEvent>().Single();

        @event.UKPRN.Should().Be(request.OnProgramme.Ukprn);
        @event.ULN.Should().Be(long.Parse(request.Learner.Uln));
        @event.LearningKey.Should().Be(request.LearningKey);
        @event.CourseCode.Should().Be(request.OnProgramme.TrainingCode);
        @event.LearningType.Should().Be(LearningType.Apprenticeship);
        @event.CourseType.Should().Be(CourseType.Apprenticeship);
    }
}
