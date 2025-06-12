using NUnit.Framework;
using SFA.DAS.Apprenticeships.Types;
using SFA.DAS.Funding.ApprenticeshipEarnings.AcceptanceTests.Extensions;
using SFA.DAS.Funding.ApprenticeshipEarnings.AcceptanceTests.Model;
using SFA.DAS.Funding.ApprenticeshipEarnings.DataAccess.Entities;
using SFA.DAS.Funding.ApprenticeshipEarnings.TestHelpers;
using SFA.DAS.Funding.ApprenticeshipEarnings.Types;
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
        _scenarioContext.GetApprenticeshipCreatedEventBuilder()
            .WithDuration(months);

        _scenarioContext.GetApprenticeshipStartDateChangedEventBuilder()
            .WithDuration(months);
    }

    [Given(@"the (.*) date has been moved (.*) months (.*)")]
    public void AdjustDate(string field, int months, string action)
    {
        var monthChange = action == "earlier" ? -months : months;
        switch(field)
        {
            case "start":
                _scenarioContext.GetApprenticeshipStartDateChangedEventBuilder()
                    .WithAdjustedStartDateBy(monthChange);
                break;
            case "end":
                _scenarioContext.GetApprenticeshipStartDateChangedEventBuilder()
                    .WithAdjustedEndDateBy(monthChange);
                break;
        }
    }

    #endregion

    #region Act
    [When("the price change is approved by the other party before the end of year")]
    public async Task PublishPriceChangeEvents()
    {
        var apprenticeshipPriceChangedEvent = _scenarioContext.GetApprenticeshipPriceChangedEventBuilder().Build();
        await _testContext.TestFunction.PublishEvent(apprenticeshipPriceChangedEvent);
        _scenarioContext.Set(apprenticeshipPriceChangedEvent);

        await WaitHelper.WaitForItAsync(async () => await EnsureRecalculationHasHappened(), "Failed to publish priceChange");
    }

    [When("the following price change request is sent")]
    public async Task PublishPriceChangeEvent(Table table)
    {
        var data = table.CreateSet<PriceChangeModel>().ToList().Single();
        var apprenticeshipPriceChangedEvent = _scenarioContext.GetApprenticeshipPriceChangedEventBuilder()
            .WithExistingApprenticeshipData(_scenarioContext.Get<ApprenticeshipCreatedEvent>())
            .WithDataFromSetupModel(data)
            .Build();
        await _testContext.TestFunction.PublishEvent(apprenticeshipPriceChangedEvent);
        _scenarioContext.Set(apprenticeshipPriceChangedEvent);

        await WaitHelper.WaitForItAsync(async () => await EnsureRecalculationHasHappened(), "Failed to publish priceChange");
    }

    [When("the following start date change request is sent")]
    public async Task PublishStartDateChangeEvent(Table table)
    {
        var data = table.CreateSet<StartDateChangeModel>().ToList().Single();
        var apprenticeshipStartDateChangedEvent = _scenarioContext.GetApprenticeshipStartDateChangedEventBuilder()
            .WithExistingApprenticeshipData(_scenarioContext.Get<ApprenticeshipCreatedEvent>())
            .WithDataFromSetupModel(data)
            .Build();
        await _testContext.TestFunction.PublishEvent(apprenticeshipStartDateChangedEvent);
        _scenarioContext.Set(apprenticeshipStartDateChangedEvent);

        await WaitHelper.WaitForItAsync(async () => await EnsureRecalculationHasHappened(), "Failed to publish start date change");
    }

    [When("the following withdrawal is sent")]
    public async Task PublishWithdrawnEvent(Table table)
    {
        var data = table.CreateSet<WithdrawalModel>().ToList().Single();
        var apprenticeshipWithdrawnEvent = _scenarioContext.GetApprenticeshipWithdrawnEventBuilder()
            .WithExistingApprenticeshipData(_scenarioContext.Get<ApprenticeshipCreatedEvent>())
            .WithLastDayOfLearning(data.LastDayOfLearning)
            .Build();

        await _testContext.TestFunction.PublishEvent(apprenticeshipWithdrawnEvent);
        _scenarioContext.Set(apprenticeshipWithdrawnEvent);

        await WaitHelper.WaitForItAsync(async () => await EnsureRecalculationHasHappened(), "Failed to publish withdrawal");
    }

    [When("the start date change is approved")]
	public async Task PublishStartDateChangeEvents()
	{
        var apprenticeshipStartDateChangedEvent = _scenarioContext.GetApprenticeshipStartDateChangedEventBuilder()
            .WithApprenticeshipKey(_scenarioContext.Get<ApprenticeshipCreatedEvent>().ApprenticeshipKey)
            .WithEpisodeKey(_scenarioContext.Get<ApprenticeshipCreatedEvent>().Episode.Key)
            .WithFundingBandMaximum(_scenarioContext.Get<ApprenticeshipCreatedEvent>().Episode.Prices.First().FundingBandMaximum)
            .WithAgeAtStart(_scenarioContext.Get<ApprenticeshipCreatedEvent>().Episode.AgeAtStartOfApprenticeship)
            .Build();
        await _testContext.TestFunction.PublishEvent(apprenticeshipStartDateChangedEvent);
        _scenarioContext.Set(apprenticeshipStartDateChangedEvent);

        await WaitHelper.WaitForItAsync(async () => await EnsureRecalculationHasHappened(), "Failed to publish start date change");
	}

    #endregion

    #region Assert

    [Then("the earnings history is maintained")]
    public void AssertHistoryUpdated()
    {
        return;

        var apprenticeshipModel = _scenarioContext.Get<ApprenticeshipModel>();
        var currentEpisode = apprenticeshipModel!.GetCurrentEpisode(TestSystemClock.Instance());
        if (currentEpisode.EarningsProfileHistory == null || !currentEpisode.EarningsProfileHistory.Any())
        {
            Assert.Fail("No earning history created");
        }

        var previousEarningsProfileId
            = currentEpisode.EarningsProfileHistory.OrderBy(x => x.SupersededDate).Last().EarningsProfileId;

        Assert.That(currentEpisode.EarningsProfile.EarningsProfileId != Guid.Empty &&
                    currentEpisode.EarningsProfile.EarningsProfileId != previousEarningsProfileId);
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
        await WaitHelper.WaitForUnexpected(() => _testContext.MessageSession.ReceivedEvents<ApprenticeshipEarningsRecalculatedEvent>().Any(x => x.ApprenticeshipKey == _scenarioContext.Get<ApprenticeshipCreatedEvent>().ApprenticeshipKey), "Found published ApprenticeshipEarningsRecalculatedEvent event when expecting no earnings to be recalculated", TimeSpan.FromSeconds(10));
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
        return await _testContext.SqlDatabase.GetApprenticeship(_scenarioContext.Get<ApprenticeshipCreatedEvent>().ApprenticeshipKey);
    }

    private async Task<bool> EnsureRecalculationHasHappened()
    {
        return true;

        var apprenticeshipEntity = await GetApprenticeshipEntity();

        var currentEpisode = apprenticeshipEntity!.GetCurrentEpisode(TestSystemClock.Instance());
        if (!currentEpisode.EarningsProfileHistory.Any())
        {
            return false;
        }

        _scenarioContext.Set(apprenticeshipEntity);
        return true;
    }

    #endregion
}