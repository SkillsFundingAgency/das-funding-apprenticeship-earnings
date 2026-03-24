using NUnit.Framework;
using SFA.DAS.Funding.ApprenticeshipEarnings.AcceptanceTests.Extensions;
using SFA.DAS.Funding.ApprenticeshipEarnings.AcceptanceTests.Model;
using SFA.DAS.Funding.ApprenticeshipEarnings.Command.UpdateShortCourseOnProgrammeCommand;
using SFA.DAS.Funding.ApprenticeshipEarnings.DataAccess.Entities;
using SFA.DAS.Funding.ApprenticeshipEarnings.DataAccess.Entities.Apprenticeship;
using SFA.DAS.Funding.ApprenticeshipEarnings.DataAccess.Entities.ShortCourse;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Models.ShortCourse;
using SFA.DAS.Funding.ApprenticeshipEarnings.Queries.GetFm99ShortCourseEarnings;
using SFA.DAS.Funding.ApprenticeshipEarnings.Queries.GetShortCourse;
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

        var response = await _testContext.TestInnerApi.Get<GetFm99ShortCourseEarningsResponse>(
            $"/fm99/{learningKey}/shortCourses?ukprn={ukprn}");

        _scenarioContext.Set(response);
    }

    [Then("the earnings response contains")]
    public void ThenTheEarningsResponseContains(Table table)
    {
        var response = _scenarioContext.Get<GetFm99ShortCourseEarningsResponse>();
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

        var response = await _testContext.TestInnerApi.Put<UpdateShortCourseOnProgrammeRequest, UpdateShortCourseOnProgrammeResponse>($"/{shortCourseCreateRequest.LearningKey}/shortCourses/on-programme", updateOnProgrammeRequest);

        var shortCourseEntity = await GetLearningEntity(shortCourseCreateRequest.LearningKey);

        _scenarioContext.Set(shortCourseEntity);
        _scenarioContext.Set(updateOnProgrammeRequest);
        _scenarioContext.Set(response);
    }

    [Then(@"the following data is returned from the put request")]
    public void ThenTheFollowingDataIsReturnedFromThePutRequest(Table table)
    {
        var data = table.CreateSet<ShortCourseUpdateResponseExpectationModel>().ToList();

        var response = _scenarioContext.Get<UpdateShortCourseOnProgrammeResponse>();
        AssertShortCourseResponse(data, response);
    }

    [Then(@"On programme short course earnings are persisted as follows")]
    public async Task ThenOnProgrammeShortCourseEarningsArePersistedAsFollows(Table table)
    {
        var learningKey = _scenarioContext.Get<CreateUnapprovedShortCourseLearningRequest>().LearningKey;

        var data = table.CreateSet<EarningDbExpectationModel>().ToList();
        ShortCourseLearningEntity? updatedEntity;

        updatedEntity = await _testContext.SqlDatabase.GetShortCourseLearning(learningKey);
        var earningsInDb = updatedEntity.Episodes.First().EarningsProfile.Instalments.OrderBy(x => x.AcademicYear).ThenBy(x => x.DeliveryPeriod);

        earningsInDb.Should().HaveCount(data.Count);

        foreach (var expectedEarning in data)
        {
            earningsInDb.Should()
                .Contain(x => Math.Round(x.Amount, 2) == Math.Round(expectedEarning.Amount, 2)
                              && x.AcademicYear == expectedEarning.AcademicYear
                              && x.DeliveryPeriod == expectedEarning.DeliveryPeriod
                              && (expectedEarning.Type == null || Enum.Parse<ShortCourseInstalmentType>(expectedEarning.Type) == Enum.Parse<ShortCourseInstalmentType>(x.Type))
                    , $"Expected earning not found: {Newtonsoft.Json.JsonConvert.SerializeObject(expectedEarning)}");
        }
    }

    [When(@"a Get Short Course request is made for the short course")]
    public async Task WhenAGetShortCourseRequestIsMadeForTheShortCourse()
    {
        var shortCourseCreateRequest = _scenarioContext.Get<CreateUnapprovedShortCourseLearningRequest>();

        var entity = await _testContext.SqlDatabase.GetShortCourseLearning(shortCourseCreateRequest.LearningKey);
        var response = await _testContext.TestInnerApi.Get<GetShortCourseResponse>($"/{shortCourseCreateRequest.LearningKey}/shortCourses?ukprn={entity.Episodes.Single().Ukprn}");

        _scenarioContext.Set(response);
    }

    [Then(@"the following data is returned from the get request")]
    public void ThenTheFollowingDataIsReturnedFromTheGetRequest(Table table)
    {
        var data = table.CreateSet<ShortCourseUpdateResponseExpectationModel>().ToList();
        var response = _scenarioContext.Get<GetShortCourseResponse>();
        AssertShortCourseResponse(data, response);
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

    private async Task<ShortCourseLearningEntity> GetLearningEntity(Guid learningKey)
    {
        return await _testContext.SqlDatabase.GetShortCourseLearning(learningKey);
    }

    private void AssertShortCourseResponse(
        List<ShortCourseUpdateResponseExpectationModel> expectations,
        SFA.DAS.Funding.ApprenticeshipEarnings.DataTransferObjects.ShortCourseEarnings response)
    {
        foreach (var expected in expectations)
        {
            response.Instalments.Should().ContainSingle(e =>
                e.CollectionYear == expected.CollectionYear &&
                e.CollectionPeriod == expected.CollectionPeriod &&
                e.Amount == expected.Amount &&
                e.Type == expected.Type &&
                e.IsPayable == expected.IsPayable);
        }
    }

}