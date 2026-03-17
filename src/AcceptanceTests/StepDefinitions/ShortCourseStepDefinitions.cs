using NUnit.Framework;
using SFA.DAS.Funding.ApprenticeshipEarnings.AcceptanceTests.Extensions;
using SFA.DAS.Funding.ApprenticeshipEarnings.AcceptanceTests.Model;
using SFA.DAS.Funding.ApprenticeshipEarnings.DataAccess.Entities;
using SFA.DAS.Funding.ApprenticeshipEarnings.DataAccess.Entities.Apprenticeship;
using SFA.DAS.Funding.ApprenticeshipEarnings.DataAccess.Entities.ShortCourse;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Models.ShortCourse;
using SFA.DAS.Funding.ApprenticeshipEarnings.Queries.GetShortCourseEarnings;
using SFA.DAS.Funding.ApprenticeshipEarnings.TestHelpers;
using SFA.DAS.Funding.ApprenticeshipEarnings.Types;
using SFA.DAS.Learning.Types;
using System.Text.Json;
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
        var updatedEntity = await _testContext.SqlDatabase.GetShortCourseLearning(request.LearningKey);

        JsonSerializer
            .Deserialize<CreateUnapprovedShortCourseLearningRequest>(updatedEntity.Episodes.First().EarningsProfile.CalculationData)
            .Should().BeEquivalentTo(request);
    }

    [When("I request the short course earnings")]
    public async Task WhenIRequestTheShortCourseEarnings()
    {
        var request = _scenarioContext.Get<CreateUnapprovedShortCourseLearningRequest>();
        var learningKey = request.LearningKey;
        var ukprn = request.OnProgramme.Ukprn;

        var response = await _testContext.TestInnerApi.Get<GetShortCourseEarningsResponse>(
            $"/{learningKey}/shortCourses?ukprn={ukprn}");

        _scenarioContext.Set(response);
    }

    [Then("the earnings response contains")]
    public void ThenTheEarningsResponseContains(Table table)
    {
        var response = _scenarioContext.Get<GetShortCourseEarningsResponse>();
        var expectedEarnings = table.CreateSet<EarningsAssertionModel>().ToList();

        response.Earnings.Should().HaveCount(expectedEarnings.Count);

        foreach (var expected in expectedEarnings)
        {
            response.Earnings.Should().ContainSingle(e =>
                e.CollectionYear == expected.CollectionYear &&
                e.CollectionPeriod == expected.CollectionPeriod &&
                e.Amount == expected.Amount &&
                e.Type == expected.Type);
        }
    }

    [Then("the short course earnings history is maintained")]
    public async Task AssertHistoryUpdated()
    {
        var request = _scenarioContext.Get<CreateUnapprovedShortCourseLearningRequest>();
        var updatedEntity = await _testContext.SqlDatabase.GetShortCourseLearning(request.LearningKey);
        var episode = updatedEntity.Episodes.Single();

        var history = await _testContext.SqlDatabase.GetShortCourseHistory(episode.EarningsProfile.EarningsProfileId);

        if (history.Count == 0)
        {
            Assert.Fail("No earning history created");
        }

        history.First().Version.Should().Be(episode.EarningsProfile.Version);
    }

    [When(@"Short Course Update OnProgramme is triggered with")]
    public async Task WhenShortCourseUpdateOnProgrammeIsTriggeredWith(Table table)
    {
        var shortCourseCreateRequest = _scenarioContext.Get<CreateUnapprovedShortCourseLearningRequest>();

        var data = GetUpdateOnProgrammeModel(table);

        var updateOnProgrammeRequest = _scenarioContext.GetShortCourseUpdateOnProgrammeRequestBuilder()
            .WithExistingData(shortCourseCreateRequest)
            .WithDataFromSetupModel(data)
            .Build();

        await _testContext.TestInnerApi.Put($"/{shortCourseCreateRequest.LearningKey}/shortCourses/on-programme", updateOnProgrammeRequest);

        var shortCourseEntity = await GetLearningEntity(shortCourseCreateRequest.LearningKey);

        _scenarioContext.Set(shortCourseEntity);
        _scenarioContext.Set(updateOnProgrammeRequest);
    }

    private UpdateShortCourseOnProgrammeModel GetUpdateOnProgrammeModel(Table table)
    {
        var data = table.CreateSet<KeyValueModel>().ToList();
        var model = new UpdateShortCourseOnProgrammeModel();

        foreach (var item in data)
        {
            switch (item.Key)
            {
                case nameof(UpdateShortCourseOnProgrammeModel.Milestones):
                    model.Milestones.SetValue(item.Value.ToEnumList<Milestone>());
                    break;

                case nameof(UpdateShortCourseOnProgrammeModel.WithdrawalDate):
                    model.WithdrawalDate.SetValue(item.ToNullableDateTime());
                    break;
            }
        }

        return model;
    }

    [Then(@"the short course instalment payability is")]
    public async Task ThenTheShortCourseInstalmentPayabilityIs(Table table)
    {
        var request = _scenarioContext.Get<CreateUnapprovedShortCourseLearningRequest>();
        var entity = await _testContext.SqlDatabase.GetShortCourseLearning(request.LearningKey);
        var instalments = entity!.Episodes.First().EarningsProfile.Instalments;

        var expected = table.CreateSet<InstalmentPayabilityExpectation>().ToList();

        foreach (var exp in expected)
        {
            var instalment = instalments.Single(i => Enum.Parse<ShortCourseInstalmentType>(i.Type) == exp.Type);
            instalment.IsPayable.Should().Be(exp.IsPayable, $"{exp.Type} instalment should have IsPayable={exp.IsPayable}");
        }
    }

    private async Task<ShortCourseLearningEntity> GetLearningEntity(Guid learningKey)
    {
        return await _testContext.SqlDatabase.GetShortCourseLearning(learningKey);
    }

    private class InstalmentPayabilityExpectation
    {
        public ShortCourseInstalmentType Type { get; set; }
        public bool IsPayable { get; set; }
    }
}