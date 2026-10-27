using FluentAssertions;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Models.Apprenticeship;
using SFA.DAS.Funding.ApprenticeshipEarnings.Types;
using SFA.DAS.Payments.EarningEvents.Messages.External;
using SFA.DAS.Payments.EarningEvents.Messages.External.Commands;
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

    [Then("no on-programme payments event is sent to pv2 for the current earnings")]
    public async Task ThenNoOnProgrammePaymentsEventIsSentForTheCurrentEarnings()
    {
        var currentVersion = await GetCurrentEarningsProfileVersion();

        var onProgrammeEvents = _testContext.MessageSession.ReceivedEvents<CalculateGrowthAndSkillsPayments>()
            .Where(x => x.Training.CourseType == CourseType.Apprenticeship)
            .ToList();

        onProgrammeEvents.Should().NotBeEmpty("the earnings sent on approval should have been recorded"); // Earnings were sent on approval
        onProgrammeEvents.Should().NotContain(x => x.EarningsId == currentVersion); // No earnings for this version, because the episode is withheld
    }

    [Then("an english and maths payments event is sent to pv2 for the current earnings")]
    public async Task ThenAnEnglishAndMathsPaymentsEventIsSentForTheCurrentEarnings()
    {
        var request = _scenarioContext.Get<CreateUnapprovedApprenticeshipLearningRequest>();
        var dbEntity = await _testContext.SqlDatabase.GetApprenticeshipLearning(request.LearningKey);
        var episode = ApprenticeshipLearning.Get(dbEntity!).GetEpisode(request.EpisodeKey);
        var course = episode.EarningsProfile!.MathsAndEnglishCourses.Single();

        _testContext.MessageSession.ReceivedEvents<CalculateGrowthAndSkillsPayments>()
            .Should().Contain(x => x.Training.LearningType == LearningType.MathsAndEnglish && x.EarningsId == course.Version);
    }

    private async Task<Guid> GetCurrentEarningsProfileVersion()
    {
        var request = _scenarioContext.Get<CreateUnapprovedApprenticeshipLearningRequest>();
        var dbEntity = await _testContext.SqlDatabase.GetApprenticeshipLearning(request.LearningKey);
        var episode = ApprenticeshipLearning.Get(dbEntity!).GetEpisode(request.EpisodeKey);

        return episode.EarningsProfile!.Version;
    }
}
