using SFA.DAS.Funding.ApprenticeshipEarnings.AcceptanceTests.Model;
using SFA.DAS.Funding.ApprenticeshipEarnings.Command.UpdateEnglishAndMathsCommand;
using SFA.DAS.Learning.Types;
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

    [Given(@"the following english and maths course information is provided")]
    [When(@"the following english and maths completion change request is sent")]
    [When(@"the following english and maths course information is provided")]
    [When(@"the following english and maths withdrawal change request is sent")]
    public async Task GivenTheFollowingMathsAndEnglishCourseInformationIsProvided(Table table)
    {
        var items = table.CreateSet<EnglishAndMathsItem>().ToList();
        var request = new UpdateEnglishAndMathsRequest
        {
            EnglishAndMaths = items
        };
        await _testContext.TestInnerApi.Put($"/learning/{_scenarioContext.Get<LearningCreatedEvent>().LearningKey}/english-and-maths", request);
    }

    [Then(@"english and maths instalments are persisted as follows")]
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
            var expectedInstalmentCount = data.Count(d => d.Course == course);
            var courseInDb = mathsAndEnglishCoursesInDb.SingleOrDefault(x => x.Course.TrimEnd() == course);
            courseInDb.Should().NotBeNull();
            courseInDb.Instalments.Should().HaveCount(expectedInstalmentCount);
        }

        // Check individual instalments
        foreach (var expectedInstalment in data)
        {
            var courseInDb = mathsAndEnglishCoursesInDb.SingleOrDefault(x => x.Course.TrimEnd() == expectedInstalment.Course);
            courseInDb.Should().NotBeNull();

            var hasInstalment = courseInDb.Instalments.Any(x => x.Amount == expectedInstalment.Amount
                                                      && x.AcademicYear == expectedInstalment.AcademicYear
                                                      && x.DeliveryPeriod == expectedInstalment.DeliveryPeriod
                                                      && x.Type == expectedInstalment.Type);

            hasInstalment.Should().BeTrue($"Expected to find instalment for course {expectedInstalment.Course} with Amount {expectedInstalment.Amount}, AcademicYear {expectedInstalment.AcademicYear}, DeliveryPeriod {expectedInstalment.DeliveryPeriod}, Type {expectedInstalment.Type}");
            courseInDb.Instalments.Should()
                .Contain(x => x.Amount == expectedInstalment.Amount
                              && x.AcademicYear == expectedInstalment.AcademicYear
                              && x.DeliveryPeriod == expectedInstalment.DeliveryPeriod
                              && x.Type == expectedInstalment.Type);
        }
    }

    [Then(@"no english and maths earnings are persisted")]
    public async Task ThenNoMathsAndEnglishInstalmentsArePersisted()
    {
        var learningCreatedEvent = _scenarioContext.Get<LearningCreatedEvent>();

        var updatedEntity = await _testContext.SqlDatabase.GetApprenticeship(learningCreatedEvent.LearningKey);

        var mathsAndEnglishInstalmentsInDb = updatedEntity.Episodes.First().EarningsProfile.MathsAndEnglishCourses.SelectMany(x => x.Instalments);

        mathsAndEnglishInstalmentsInDb.Should().BeEmpty();
    }
}
