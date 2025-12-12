using NUnit.Framework;
using SFA.DAS.Funding.ApprenticeshipEarnings.AcceptanceTests.Model;
using SFA.DAS.Funding.ApprenticeshipEarnings.Command.UpdateEnglishAndMathsCommand;
using SFA.DAS.Learning.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TechTalk.SpecFlow.Assist;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.AcceptanceTests.StepDefinitions;

[Binding]
public class EnglishAndMathsStepDefinitions
{
    private readonly ScenarioContext _scenarioContext;
    private readonly TestContext _testContext;

    public EnglishAndMathsStepDefinitions(ScenarioContext scenarioContext, TestContext testContext)
    {
        _scenarioContext = scenarioContext;
        _testContext = testContext;
    }

    [Given(@"the following maths and english course information is provided")]
    [When(@"the following maths and english completion change request is sent")]
    [When(@"the following maths and english course information is provided")]
    [When(@"the following maths and english withdrawal change request is sent")]
    public async Task GivenTheFollowingMathsAndEnglishCourseInformationIsProvided(Table table)
    {
        var items = table.CreateSet<EnglishAndMathsItem>().ToList();
        var request = new UpdateEnglishAndMathsRequest
        {
            EnglishAndMaths = items
        };
        await _testContext.TestInnerApi.Put($"/learning/{_scenarioContext.Get<LearningCreatedEvent>().LearningKey}/english-and-maths", request);
    }

    [Then(@"Maths and english instalments are persisted as follows")]
    public async Task ThenMathsAndEnglishInstalmentsArePersistedAsFollows(Table table)
    {
        var data = table.CreateSet<MathsAndEnglishInstalmentDbExpectationModel>().ToList();

        var learningCreatedEvent = _scenarioContext.Get<LearningCreatedEvent>();

        var updatedEntity = await _testContext.SqlDatabase.GetApprenticeship(learningCreatedEvent.LearningKey);

        var mathsAndEnglishCoursesInDb = updatedEntity.Episodes.First().EarningsProfile.MathsAndEnglishCourses;

        // Check number of instalments per course
        var expectedCourses = data.Select(d => d.Course).Distinct().ToList();
        foreach (var course in expectedCourses)
        {
            var expectedInstalmentCount = data.Count(d => d.Course == course && d.IsAfterLearningEnded != true);
            var courseInDb = mathsAndEnglishCoursesInDb.SingleOrDefault(x => x.Course.TrimEnd() == course);
            courseInDb.Should().NotBeNull();
            courseInDb.Instalments.Where(x => !x.IsAfterLearningEnded).Should().HaveCount(expectedInstalmentCount);
        }

        // Check individual instalments
        foreach (var expectedInstalment in data)
        {
            var courseInDb = mathsAndEnglishCoursesInDb.SingleOrDefault(x => x.Course.TrimEnd() == expectedInstalment.Course);
            courseInDb.Should().NotBeNull();

            courseInDb.Instalments.Should()
                .Contain(x => x.Amount == expectedInstalment.Amount
                              && x.AcademicYear == expectedInstalment.AcademicYear
                              && x.DeliveryPeriod == expectedInstalment.DeliveryPeriod
                              && x.Type == expectedInstalment.Type
                              && (!expectedInstalment.IsAfterLearningEnded.HasValue || x.IsAfterLearningEnded == expectedInstalment.IsAfterLearningEnded.Value));
        }
    }

    [Then(@"no Maths and English earnings are persisted")]
    public async Task ThenNoMathsAndEnglishInstalmentsArePersisted()
    {
        var learningCreatedEvent = _scenarioContext.Get<LearningCreatedEvent>();

        var updatedEntity = await _testContext.SqlDatabase.GetApprenticeship(learningCreatedEvent.LearningKey);

        var mathsAndEnglishInstalmentsInDb = updatedEntity.Episodes.First().EarningsProfile.MathsAndEnglishCourses.SelectMany(x => x.Instalments);

        mathsAndEnglishInstalmentsInDb.Should().BeEmpty();
    }

    [Then(@"all Maths and English earnings are soft deleted")]
    public async Task ThenAllMathsAndEnglishInstalmentsAreSoftDeleted()
    {
        var learningCreatedEvent = _scenarioContext.Get<LearningCreatedEvent>();

        var updatedEntity = await _testContext.SqlDatabase.GetApprenticeship(learningCreatedEvent.LearningKey);

        var mathsAndEnglishInstalmentsInDb = updatedEntity.Episodes.First().EarningsProfile.MathsAndEnglishCourses.SelectMany(x => x.Instalments);

        mathsAndEnglishInstalmentsInDb.Should().OnlyContain(x => x.IsAfterLearningEnded,
            "Expected all maths and english instalments to be soft deleted");
    }

}
