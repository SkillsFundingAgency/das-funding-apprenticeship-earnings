using NUnit.Framework;
using SFA.DAS.Funding.ApprenticeshipEarnings.AcceptanceTests.Extensions;
using SFA.DAS.Funding.ApprenticeshipEarnings.AcceptanceTests.Model;
using SFA.DAS.Funding.ApprenticeshipEarnings.Command.SaveMathsAndEnglishCommand;
using SFA.DAS.Funding.ApprenticeshipEarnings.DataAccess.Entities;
using SFA.DAS.Funding.ApprenticeshipEarnings.TestHelpers;
using SFA.DAS.Funding.ApprenticeshipEarnings.Types;
using SFA.DAS.Learning.Types;
using TechTalk.SpecFlow.Assist;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.AcceptanceTests.StepDefinitions;

[Binding]
public class RecalculateEarningsStepDefinitions
{
    private readonly ScenarioContext _scenarioContext;
    private readonly TestContext _testContext;

    public RecalculateEarningsStepDefinitions(ScenarioContext scenarioContext, TestContext testContext)
    {
        _scenarioContext = scenarioContext;
        _testContext = testContext;
    }


    #region Arrange
    [Given(@"there are (.*) earnings")]
    public void SetDuration(int months)
    {
        _scenarioContext.GetLearningCreatedEventBuilder()
            .WithDuration(months);

        _scenarioContext.GetLearningStartDateChangedEventBuilder()
            .WithDuration(months);
    }

    [Given(@"the (.*) date has been moved (.*) months (.*)")]
    public void AdjustDate(string field, int months, string action)
    {
        var monthChange = action == "earlier" ? -months : months;
        switch(field)
        {
            case "start":
                _scenarioContext.GetLearningStartDateChangedEventBuilder()
                    .WithAdjustedStartDateBy(monthChange);
                break;
            case "end":
                _scenarioContext.GetLearningStartDateChangedEventBuilder()
                    .WithAdjustedEndDateBy(monthChange);
                break;
        }
    }

    #endregion

    #region Act
    [When("the price change is approved by the other party before the end of year")]
    public async Task PublishPriceChangeEvents()
    {
        var learningPriceChangedEvent = _scenarioContext.GetLearningPriceChangedEventBuilder().Build();
        await _testContext.TestFunction.PublishEvent(learningPriceChangedEvent);
        _scenarioContext.Set(learningPriceChangedEvent);

        await WaitHelper.WaitForItAsync(async () => await EnsureRecalculationHasHappened(), "Failed to publish priceChange");
    }

    [When("the following price change request is sent")]
    public async Task SendPriceChangeRequest(Table table)
    {
        var data = table.CreateSet<PriceChangeModel>().ToList().Single();
        var learningPriceChangedRequest = _scenarioContext.GetLearningPriceChangedRequestBuilder()
            .WithExistingApprenticeshipData(_scenarioContext.Get<LearningCreatedEvent>())
            .WithDataFromSetupModel(data)
            .Build();

        await _testContext.TestInnerApi.Patch($"/apprenticeship/{_scenarioContext.Get<LearningCreatedEvent>().LearningKey}/prices", learningPriceChangedRequest);

        await WaitHelper.WaitForItAsync(async () => await EnsureRecalculationHasHappened(), "Failed to publish priceChange");
    }

    [When("the following start date change request is sent")]
    public async Task SendSavePricesRequestWithUpdatedDates(Table table)
    {
        var data = table.CreateSet<StartDateChangeModel>().ToList().Single();
        var savePricesRequest = _scenarioContext.GetLearningStartDateChangedEventBuilder()
            .WithExistingApprenticeshipData(_scenarioContext.Get<LearningCreatedEvent>())
            .WithDataFromSetupModel(data)
            .Build();

        await _testContext.TestInnerApi.Patch($"/apprenticeship/{_scenarioContext.Get<LearningCreatedEvent>().LearningKey}/prices", savePricesRequest);

        await WaitHelper.WaitForItAsync(async () => await EnsureRecalculationHasHappened(), "Failed to publish priceChange");

        _scenarioContext.Set(savePricesRequest);
    }

    [When("the following withdrawal is sent")]
    public async Task PublishWithdrawnEvent(Table table)
    {
        var data = table.CreateSet<WithdrawalModel>().ToList().Single();
        var learningWithdrawnEvent = _scenarioContext.GetLearningWithdrawnEventBuilder()
            .WithExistingApprenticeshipData(_scenarioContext.Get<LearningCreatedEvent>())
            .WithLastDayOfLearning(data.LastDayOfLearning)
            .Build();

        await _testContext.TestFunction.PublishEvent(learningWithdrawnEvent);
        _scenarioContext.Set(learningWithdrawnEvent);

        await WaitHelper.WaitForItAsync(async () => await EnsureRecalculationHasHappened(), "Failed to publish withdrawal");
    }

    [When("the updated prices with new dates request is sent")]
    public async Task SendUpdatedPriceChangeRequest()
    {
        var savePricesRequest = _scenarioContext.GetLearningStartDateChangedEventBuilder()
                    .WithLearningKey(_scenarioContext.Get<LearningCreatedEvent>().LearningKey)
                    .WithEpisodeKey(_scenarioContext.Get<LearningCreatedEvent>().Episode.Key)
                    .WithFundingBandMaximum(_scenarioContext.Get<LearningCreatedEvent>().Episode.Prices.First().FundingBandMaximum)
                    .WithAgeAtStart(_scenarioContext.Get<LearningCreatedEvent>().Episode.AgeAtStartOfLearning)
                    .Build();

        await _testContext.TestInnerApi.Patch($"/apprenticeship/{_scenarioContext.Get<LearningCreatedEvent>().LearningKey}/prices", savePricesRequest);

        await WaitHelper.WaitForItAsync(async () => await EnsureRecalculationHasHappened(), "Failed to publish priceChange");
    }


    #endregion

    #region Assert

    [Then("the earnings history is maintained")]
    public async Task AssertHistoryUpdated()
    {
        await WaitHelper.WaitForItAsync(async () => await EnsureRecalculationHasHappened(), "Unable to await recalculation");

        var apprenticeshipModel = _scenarioContext.Get<ApprenticeshipModel>();
        var currentEpisode = apprenticeshipModel!.GetCurrentEpisode(TestSystemClock.Instance());
        
        var history = await _testContext.SqlDatabase.GetHistory(currentEpisode.EarningsProfile.EarningsProfileId);

        if (history.Count == 0)
        {
            Assert.Fail("No earning history created");
        }

        history.First().Version.Should().Be(currentEpisode.EarningsProfile.Version);
    }

    [Then("there are (.*) records in earning profile history")]
    public async Task AssertHistoryUpdated(int numberOfRecords)
    {
        await WaitHelper.WaitForItAsync(async () => await EnsureRecalculationHasHappened(), "Failed to detect Earnings recalculation");

        var apprenticeshipModel = _scenarioContext.Get<ApprenticeshipModel>();
        var currentEpisode = apprenticeshipModel!.GetCurrentEpisode(TestSystemClock.Instance());

        var history = await _testContext.SqlDatabase.GetHistory(currentEpisode.EarningsProfile.EarningsProfileId);

        if (history.Count == 0)
        {
            Assert.Fail("No earning history created");
        }

        history.Count.Should().Be(numberOfRecords);
    }

    [Then(@"there are (.*) earnings")]
    public void AssertExpectedNumberOfEarnings(int expectedNumberOfEarnings)
    {
        var apprenticeshipModel = _scenarioContext.Get<ApprenticeshipModel>();
        var currentEpisode = apprenticeshipModel!.GetCurrentEpisode(TestSystemClock.Instance());

        var matchingInstalments = currentEpisode.EarningsProfile.Instalments.Count;

        if(matchingInstalments != expectedNumberOfEarnings)
        {
            Assert.Fail($"Expected to find {expectedNumberOfEarnings} instalments but found {matchingInstalments}");
        }
    }

    [Then(@"Earnings are not recalculated for that apprenticeship")]
    public async Task AssertNoEarningsGeneratedEvent()
    {
        await WaitHelper.WaitForUnexpected(() => _testContext.MessageSession.ReceivedEvents<ApprenticeshipEarningsRecalculatedEvent>().Any(x => x.ApprenticeshipKey == _scenarioContext.Get<LearningCreatedEvent>().LearningKey), "Found published ApprenticeshipEarningsRecalculatedEvent event when expecting no earnings to be recalculated", TimeSpan.FromSeconds(10));
    }

    private async Task<bool> EnsureApprenticeshipEntityCreated()
    {
        var apprenticeshipEntity = await GetApprenticeshipEntity();
        if (apprenticeshipEntity == null)
        {
            return false;
        }
        return true;
    }

    private async Task<ApprenticeshipModel> GetApprenticeshipEntity()
    {
        return await _testContext.SqlDatabase.GetApprenticeship(_scenarioContext.Get<LearningCreatedEvent>().LearningKey);
    }

    private async Task<bool> EnsureRecalculationHasHappened()
    {
        var apprenticeshipEntity = await GetApprenticeshipEntity();
        var currentEpisode = apprenticeshipEntity!.GetCurrentEpisode(TestSystemClock.Instance());

        var history = await _testContext.SqlDatabase.GetHistory(currentEpisode.EarningsProfile.EarningsProfileId);

        if (!history.Any())
        {
            return false;
        }

        //History always contains 1 record for the initial creation
        //Therefore, we must disregard this when looking for recalculation
        if (history.Count == 1)
        {
            return false;
        }

        _scenarioContext.Set(apprenticeshipEntity);
        return true;
    }

    #endregion
}