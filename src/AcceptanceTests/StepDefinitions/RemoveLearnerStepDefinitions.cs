using NUnit.Framework;
using SFA.DAS.Funding.ApprenticeshipEarnings.AcceptanceTests.Extensions;
using SFA.DAS.Funding.ApprenticeshipEarnings.AcceptanceTests.Model;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Models.Apprenticeship;
using SFA.DAS.Payments.EarningEvents.Messages.External;
using SFA.DAS.Payments.EarningEvents.Messages.External.Commands;
using SFA.DAS.Funding.ApprenticeshipEarnings.TestHelpers;
using SFA.DAS.Funding.ApprenticeshipEarnings.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TechTalk.SpecFlow.Assist;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.AcceptanceTests.StepDefinitions;

[Binding]
public class RemoveLearnerStepDefinitions
{
    private readonly ScenarioContext _scenarioContext;
    private readonly TestContext _testContext;

    public RemoveLearnerStepDefinitions(ScenarioContext scenarioContext, TestContext testContext)
    {
        _scenarioContext = scenarioContext;
        _testContext = testContext;
    }

    [When("the learner is removed")]
    public async Task SendWithdrawalRequest()
    {
        await _testContext.TestInnerApi.Delete($"/learning/{_scenarioContext.Get<CreateUnapprovedApprenticeshipLearningRequest>().LearningKey}");
    }

    [Then("the payments event is sent to pv2 with no earnings for the apprenticeship")]
    public async Task ThenThePaymentsEventIsSentToPv2WithNoEarningsForTheApprenticeship()
    {
        var request = _scenarioContext.Get<CreateUnapprovedApprenticeshipLearningRequest>();
        var episode = await GetEpisode();

        episode.IsRemoved.Should().BeTrue();
        episode.EarningsProfile!.Instalments.Should().BeEmpty();
        episode.EarningsProfile.AdditionalPayments.Should().BeEmpty();

        var paymentsEvents = _testContext.MessageSession.ReceivedEvents<CalculateGrowthAndSkillsPayments>()
            .Where(x => x.Training.LearningType == LearningType.Apprenticeship)
            .ToList();

        // Sanity-check payments had been told about earnings before the removal, rather than passing vacuously
        paymentsEvents.Should().Contain(x => x.Earnings.Any(), "payments should have been sent earnings when the apprenticeship was approved");

        var paymentsEvent = paymentsEvents.Last();
        paymentsEvent.EarningsId.Should().Be(episode.EarningsProfile.Version);
        paymentsEvent.Training.LearningKey.Should().Be(request.LearningKey);
        paymentsEvent.Learner.LearnerKey.Should().Be(_scenarioContext.GetLearnerKey());
        paymentsEvent.Learner.Reference.Should().Be(_scenarioContext.GetLearnerRef());
        paymentsEvent.Earnings.Should().BeEmpty();
    }

    [Then("the english and maths payments event is sent to pv2 with no earnings")]
    public async Task ThenTheEnglishAndMathsPaymentsEventIsSentToPv2WithNoEarnings()
    {
        var request = _scenarioContext.Get<CreateUnapprovedApprenticeshipLearningRequest>();
        var episode = await GetEpisode();
        var courses = episode.EarningsProfile!.MathsAndEnglishCourses;

        courses.Should().NotBeEmpty("this scenario expects an English & Maths course to have been persisted");

        foreach (var course in courses)
        {
            course.IsRemoved.Should().BeTrue();
            course.Instalments.Should().BeEmpty();
            course.AdditionalPayments.Should().BeEmpty();

            var learnAimRef = course.LearnAimRef.Trim();
            var paymentsEvent = _testContext.MessageSession.ReceivedEvents<CalculateGrowthAndSkillsPayments>()
                .LastOrDefault(x => x.Training.LearningType == LearningType.MathsAndEnglish && x.Training.CourseReference == learnAimRef);

            paymentsEvent.Should().NotBeNull($"expected an English & Maths payments event for {learnAimRef}");
            paymentsEvent!.EarningsId.Should().Be(course.Version);
            paymentsEvent.Training.LearningKey.Should().Be(course.Key);
            paymentsEvent.Learner.Reference.Should().Be(_scenarioContext.GetLearnerRef());
            paymentsEvent.Earnings.Should().BeEmpty();
        }
    }

    [Given(@"payments had previously been sent ""(.*)"" earnings for the apprenticeship")]
    public void GivenPaymentsHadPreviouslyBeenSentEarningsForTheApprenticeship(EarningType earningType)
    {
        var paymentsEvent = _testContext.MessageSession.ReceivedEvents<CalculateGrowthAndSkillsPayments>()
            .LastOrDefault(x => x.Training.LearningType == LearningType.Apprenticeship);

        paymentsEvent.Should().NotBeNull("payments should have been sent earnings when the apprenticeship was approved");
        GetEarningTypes(paymentsEvent!).Should().Contain(earningType);
    }

    [Given(@"payments had previously been sent earnings for each english and maths course")]
    public void GivenPaymentsHadPreviouslyBeenSentEarningsForEachEnglishAndMathsCourse()
    {
        foreach (var paymentsEvent in GetLatestEnglishAndMathsPaymentsEvents())
        {
            paymentsEvent.Earnings.Should().NotBeEmpty($"payments should have been sent earnings for {paymentsEvent.Training.CourseReference} when the apprenticeship was approved");
        }
    }

    [Given(@"payments had previously been sent ""(.*)"" earnings for each english and maths course")]
    public void GivenPaymentsHadPreviouslyBeenSentEarningsForEachEnglishAndMathsCourse(EarningType earningType)
    {
        foreach (var paymentsEvent in GetLatestEnglishAndMathsPaymentsEvents())
        {
            GetEarningTypes(paymentsEvent).Should().Contain(earningType);
        }
    }

    private List<CalculateGrowthAndSkillsPayments> GetLatestEnglishAndMathsPaymentsEvents()
    {
        var paymentsEvents = _testContext.MessageSession.ReceivedEvents<CalculateGrowthAndSkillsPayments>()
            .Where(x => x.Training.LearningType == LearningType.MathsAndEnglish)
            .GroupBy(x => x.Training.CourseReference)
            .Select(x => x.Last())
            .ToList();

        paymentsEvents.Should().NotBeEmpty("payments should have been sent english and maths earnings when the apprenticeship was approved");
        return paymentsEvents;
    }

    private static IEnumerable<EarningType> GetEarningTypes(CalculateGrowthAndSkillsPayments paymentsEvent)
    {
        return paymentsEvent.Earnings.SelectMany(e => e.PricePeriods).SelectMany(p => p.Periods).Select(p => p.EarningType);
    }

    private async Task<ApprenticeshipEpisode> GetEpisode()
    {
        var request = _scenarioContext.Get<CreateUnapprovedApprenticeshipLearningRequest>();
        var dbEntity = await _testContext.SqlDatabase.GetApprenticeshipLearning(request.LearningKey);
        var domainModel = ApprenticeshipLearning.Get(dbEntity!);

        return domainModel.GetEpisode(request.EpisodeKey);
    }
}
