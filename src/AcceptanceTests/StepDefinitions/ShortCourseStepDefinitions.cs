using NUnit.Framework;
using SFA.DAS.Funding.ApprenticeshipEarnings.AcceptanceTests.Extensions;
using SFA.DAS.Funding.ApprenticeshipEarnings.AcceptanceTests.Model;
using SFA.DAS.Funding.ApprenticeshipEarnings.Command.RemoveShortCourseLearningCommand;
using SFA.DAS.Funding.ApprenticeshipEarnings.Command.UpdateShortCourseOnProgrammeCommand;
using SFA.DAS.Funding.ApprenticeshipEarnings.DataAccess.Entities.ShortCourse;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Models.ShortCourse;
using SFA.DAS.Funding.ApprenticeshipEarnings.Queries.GetFm99ShortCourseEarnings;
using SFA.DAS.Funding.ApprenticeshipEarnings.Queries.GetShortCourse;
using SFA.DAS.Funding.ApprenticeshipEarnings.Types;
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
            .ToApiRequest(existingRequest?.LearningKey, existingRequest?.EpisodeKey);

        _scenarioContext.Set(request);

        await _testContext.TestInnerApi.Post($"/shortCourses", request);
    }

    [Given("a short course has been created by a new provider with the following information")]
    [When("a short course has been created by a new provider with the following information")]
    public async Task CreateUnapprovedShortCourseLearningForNewProvider(Table table)
    {
        _scenarioContext.TryGetValue<CreateUnapprovedShortCourseLearningRequest>(out var existingRequest);

        // Reuse LearningKey (same learner) but generate a fresh EpisodeKey (new provider)
        var request = table
            .CreateInstance<UnapprovedShortCourseSetupModel>()
            .ToApiRequest(existingRequest?.LearningKey, episodeKey: null);

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

        var response = await _testContext.TestInnerApi.Get<GetFm99ShortCourseEarningsResponse>(
            $"/fm99/{request.LearningKey}/shortCourses?ukprn={request.OnProgramme.Ukprn}");

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

    [Given(@"Provider A's short course has been withdrawn on (.*)")]
    public async Task GivenProviderAWithdrawnOn(DateTime withdrawalDate)
    {
        var model = new UpdateShortCourseOnProgrammeModel();
        model.WithdrawalDate.SetValue(withdrawalDate);
        await PerformOnProgrammeUpdate(model);
    }

    [When(@"Short Course Update OnProgramme is triggered with")]
    public async Task WhenShortCourseUpdateOnProgrammeIsTriggeredWith(Table table)
    {
        await PerformOnProgrammeUpdate(GetUpdateOnProgrammeModel(table));
    }

    private async Task PerformOnProgrammeUpdate(UpdateShortCourseOnProgrammeModel model)
    {
        var shortCourseCreateRequest = _scenarioContext.Get<CreateUnapprovedShortCourseLearningRequest>();

        var updateOnProgrammeRequest = _scenarioContext.GetShortCourseUpdateOnProgrammeRequestBuilder()
            .WithExistingData(shortCourseCreateRequest)
            .WithDataFromSetupModel(model)
            .Build();

        var response = await _testContext.TestInnerApi.Put<UpdateShortCourseOnProgrammeRequest, UpdateShortCourseOnProgrammeResponse>($"/{shortCourseCreateRequest.LearningKey}/shortCourses/{shortCourseCreateRequest.EpisodeKey}/on-programme", updateOnProgrammeRequest);

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

        var response = await _testContext.TestInnerApi.Get<GetShortCourseResponse>($"/{shortCourseCreateRequest.LearningKey}/shortCourses/{shortCourseCreateRequest.EpisodeKey}");

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

                case nameof(UpdateShortCourseOnProgrammeModel.CompletionDate):
                    model.CompletionDate.SetValue(item.ToNullableDateTime());
                    break;
            }
        }

        return model;
    }

    [Then(@"On programme short course earnings for the current episode are persisted as follows")]
    public async Task ThenOnProgrammeShortCourseEarningsForCurrentEpisodeArePersistedAsFollows(Table table)
    {
        var request = _scenarioContext.Get<CreateUnapprovedShortCourseLearningRequest>();
        var updatedEntity = await _testContext.SqlDatabase.GetShortCourseLearning(request.LearningKey);
        var episode = updatedEntity.Episodes.Single(e => e.Key == request.EpisodeKey);
        var earningsInDb = episode.EarningsProfile.Instalments.OrderBy(x => x.AcademicYear).ThenBy(x => x.DeliveryPeriod);

        var data = table.CreateSet<EarningDbExpectationModel>().ToList();

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

    [Then(@"the short course instalment payability is")]
    public async Task ThenTheShortCourseInstalmentPayabilityIs(Table table)
    {
        var request = _scenarioContext.Get<CreateUnapprovedShortCourseLearningRequest>();
        var entity = await _testContext.SqlDatabase.GetShortCourseLearning(request.LearningKey);
        var instalments = entity!.Episodes.First().EarningsProfile.Instalments;
        await AssertInstalmentPayability(instalments, table);
    }

    [Then(@"the short course instalment payability for the current episode is")]
    public async Task ThenTheShortCourseInstalmentPayabilityForCurrentEpisodeIs(Table table)
    {
        var request = _scenarioContext.Get<CreateUnapprovedShortCourseLearningRequest>();
        var entity = await _testContext.SqlDatabase.GetShortCourseLearning(request.LearningKey);
        var instalments = entity!.Episodes.Single(e => e.Key == request.EpisodeKey).EarningsProfile.Instalments;
        await AssertInstalmentPayability(instalments, table);
    }

    private static Task AssertInstalmentPayability(IEnumerable<ShortCourseInstalmentEntity> instalments, Table table)
    {
        var expected = table.CreateSet<InstalmentPayabilityExpectation>().ToList();

        foreach (var exp in expected)
        {
            var instalment = instalments.Single(i => Enum.Parse<ShortCourseInstalmentType>(i.Type) == exp.Type);
            instalment.IsPayable.Should().Be(exp.IsPayable, $"{exp.Type} instalment should have IsPayable={exp.IsPayable}");
        }

        return Task.CompletedTask;
    }

    [When(@"the short course learning is deleted")]
    public async Task WhenTheShortCourseLearningIsDeleted()
    {
        var request = _scenarioContext.Get<CreateUnapprovedShortCourseLearningRequest>();
        var learnerKey = Guid.NewGuid();
        var learnerRef = $"learner-{request.LearningKey:N}";
        await _testContext.TestInnerApi.Delete($"/{request.LearningKey}/shortCourses/{request.EpisodeKey}?learnerKey={learnerKey}&learnerRef={learnerRef}");
    }

    [Then(@"the short course has no instalments")]
    public async Task ThenTheShortCourseHasNoInstalments()
    {
        var request = _scenarioContext.Get<CreateUnapprovedShortCourseLearningRequest>();
        var entity = await _testContext.SqlDatabase.GetShortCourseLearning(request.LearningKey);
        entity!.Episodes.First().EarningsProfile.Instalments.Should().BeEmpty();
    }

    [Then(@"the short course learning has (\d+) episodes")]
    public async Task ThenTheShortCourseLearningHasEpisodes(int expectedCount)
    {
        var request = _scenarioContext.Get<CreateUnapprovedShortCourseLearningRequest>();
        var entity = await _testContext.SqlDatabase.GetShortCourseLearning(request.LearningKey);
        entity!.Episodes.Should().HaveCount(expectedCount);
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

    private class InstalmentPayabilityExpectation
    {
        public ShortCourseInstalmentType Type { get; set; }
        public bool IsPayable { get; set; }
    }
}