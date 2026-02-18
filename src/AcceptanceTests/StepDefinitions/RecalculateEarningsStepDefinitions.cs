using NUnit.Framework;
using SFA.DAS.Funding.ApprenticeshipEarnings.DataAccess.Entities;
using SFA.DAS.Funding.ApprenticeshipEarnings.TestHelpers;
using SFA.DAS.Funding.ApprenticeshipEarnings.Types;
using SFA.DAS.Learning.Types;

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

    #region Assert

    [Then("the earnings history is maintained")]
    public async Task AssertHistoryUpdated()
    {
        await WaitHelper.WaitForItAsync(async () => await EnsureRecalculationHasHappened(), "Unable to await recalculation");

        var apprenticeshipModel = _scenarioContext.Get<LearningModel>();
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

        var apprenticeshipModel = _scenarioContext.Get<LearningModel>();
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
        var apprenticeshipModel = _scenarioContext.Get<LearningModel>();
        var currentEpisode = apprenticeshipModel!.GetCurrentEpisode(TestSystemClock.Instance());

        var matchingInstalments = currentEpisode.EarningsProfile.Instalments.Count;

        if (matchingInstalments != expectedNumberOfEarnings)
        {
            Assert.Fail($"Expected to find {expectedNumberOfEarnings} instalments but found {matchingInstalments}");
        }
    }

    [Then(@"Earnings are not recalculated for that apprenticeship")]
    public async Task AssertNoEarningsGeneratedEvent()
    {
        await WaitHelper.WaitForUnexpected(() => _testContext.MessageSession.ReceivedEvents<ApprenticeshipEarningsRecalculatedEvent>().Any(x => x.ApprenticeshipKey == _scenarioContext.Get<LearningCreatedEvent>().LearningKey), "Found published ApprenticeshipEarningsRecalculatedEvent event when expecting no earnings to be recalculated", TimeSpan.FromSeconds(10));
    }

    private async Task<LearningModel> GetApprenticeshipEntity()
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