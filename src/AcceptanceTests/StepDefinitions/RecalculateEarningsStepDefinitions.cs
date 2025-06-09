using Newtonsoft.Json;
using NUnit.Framework;
using SFA.DAS.Apprenticeships.Types;
using SFA.DAS.Funding.ApprenticeshipEarnings.AcceptanceTests.Extensions;
using SFA.DAS.Funding.ApprenticeshipEarnings.DataAccess.Entities;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain;
using SFA.DAS.Funding.ApprenticeshipEarnings.TestHelpers;
using SFA.DAS.Funding.ApprenticeshipEarnings.Types;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.AcceptanceTests.StepDefinitions;

[Binding]
public class RecalculateEarningsStepDefinitions
{
    private readonly ScenarioContext _scenarioContext;
    private readonly TestContext _testContext;

    #region Test Values
    private DateTime _dateOfBirth = new DateTime(2000, 1, 1);
    private int _ageAtStartOfApprenticeship = 21;

    private readonly DateTime _startDate = new DateTime(2019, 09, 01);
    private readonly DateTime _endDate = new DateTime(2022, 1, 1);
    private int _expectedNumberOfInstalments = 28;
    private int _expectedNumberOfAdditionalPayments = 4;

    private readonly DateTime _startDateEarlierThanOriginal = new DateTime(2019, 08, 15);
    private readonly int _newExpectedNumberOfInstalmentsForEarlierStartDate = 29;
    private readonly DateTime _startDateLaterThanOriginal = new DateTime(2020, 01, 08);
    private readonly int _newExpectedNumberOfInstalmentsForLaterStartDate = 24;
    private readonly DateTime _startDateInNextAcademicYearToOriginal = new DateTime(2021, 12, 30);
    private readonly int _newExpectedNumberOfInstalmentsForStartDateInNextAcademicYear = 1;

    private readonly DateTime _changeRequestDate = new DateTime(2020, 1, 1);
    private readonly DateTime _effectiveFromDate = new DateTime(2020, 2, 1);

    private readonly int _originalPrice = 15000;
    private readonly int _fundingBandMaximum = 25000;

    private readonly int _newTrainingPrice = 17000;
    private readonly int _newAssessmentPrice = 3000;
    private readonly int _newTrainingPriceAboveBandMax = 26000;

    private readonly Guid _priceKey = Guid.NewGuid();
    private readonly Guid _episodeKey = Guid.NewGuid();
    private readonly Guid _apprenticeshipKey = Guid.NewGuid();
    private readonly long _employerAccountId = new Random().NextInt64();


    private EarningsProfileModel _originalEarningsProfile;

    #endregion

    public RecalculateEarningsStepDefinitions(ScenarioContext scenarioContext, TestContext testContext)
    {
        _scenarioContext = scenarioContext;
        _testContext = testContext;
    }


    #region Arrange
    [Given("a learner is (.*)")]
    public void SetLearnerToAgeAtStartOfApprenticeship(int age)
    {
        _dateOfBirth = _startDate.AddYears(age * -1);
        _ageAtStartOfApprenticeship = age;
    }

    [Given("an apprenticeship has been created")]
    public void ApprenticeshipCreated()
    {
        _scenarioContext.GetApprenticeshipCreatedEventBuilder()
            .WithApprenticeshipKey(_apprenticeshipKey)
            .WithEpisodeKey(_episodeKey)
            .WithPriceKey(_priceKey)
            .WithStartDate(_startDate)
            .WithFundingBandMaximum(_fundingBandMaximum)
            .WithDateOfBirth(_dateOfBirth)
            .WithEndDate(_endDate)
            .WithEmployerAccountId(_employerAccountId);

        _scenarioContext.GetApprenticeshipPriceChangedEventBuilder()
            .WithApprenticeshipKey(_apprenticeshipKey)
            .WithEpisodeKey(_episodeKey)
            .WithStartDate(_startDate)
            .WithEndDate(_endDate)
            .WithEffectiveFromDate(_effectiveFromDate)
            .WithApprovedDate(_changeRequestDate)
            .WithExistingPriceKey(_priceKey)
            .WithFundingBandMaximum(_fundingBandMaximum)
            .WithNewTrainingPrice(_newTrainingPrice)
            .WithNewAssessmentPrice(_newAssessmentPrice)
            .WithEmployerAccountId(_employerAccountId)
            .WithAgeAtStartOfApprenticeship(_ageAtStartOfApprenticeship);

        _scenarioContext.GetApprenticeshipStartDateChangedEventBuilder()
            .WithApprenticeshipKey(_apprenticeshipKey)
            .WithApprovedDate(_changeRequestDate)
            .WithStartDate(_startDate)
            .WithEpisodeKey(_episodeKey)
            .WithPriceKey(_priceKey)
            .WithEndDate(_endDate)
            .WithFundingBandMaximum(_fundingBandMaximum)
            .WithEmployerAccountId(_employerAccountId)
            .WithAgeAtStart(_ageAtStartOfApprenticeship);

        _scenarioContext.GetApprenticeshipWithdrawnEventBuilder()
            .WithApprenticeshipKey(_apprenticeshipKey);
    }

    [Given("the total price is below or at the funding band maximum")]
    [Given("the price change request is for a new total price up to or at the funding band maximum")]
    public void SetTotalBelowBandMaximum()
    {
        _scenarioContext.GetApprenticeshipPriceChangedEventBuilder()
            .WithNewTrainingPrice(_newTrainingPrice)
            .WithNewAssessmentPrice(_newAssessmentPrice);
    }

    [Given("the price change request is for a new total price above the funding band maximum")]
    public void SetTotalAboveBandMaximum()
    {
        _scenarioContext.GetApprenticeshipPriceChangedEventBuilder()
            .WithNewTrainingPrice(_newTrainingPriceAboveBandMax)
            .WithNewAssessmentPrice(_newAssessmentPrice);
    }

    [Given("a price change request was sent before the end of R14 of the current academic year")]
    public void SetPriceChangeApprovedDate()
    {
        _scenarioContext.GetApprenticeshipPriceChangedEventBuilder()
            .WithApprovedDate(_changeRequestDate);
    }

    [Given("a start date change request was sent before the end of R14 of the current academic year")]
    public void SetStartDateChangeApprovedDate()
    {
        _scenarioContext.GetApprenticeshipStartDateChangedEventBuilder()
            .WithApprovedDate(_changeRequestDate);
    }

    [Given("the new start date is earlier than, and in the same academic year, as the current start date")]
    public void SetEarlierStartDateChange()
    {
        _scenarioContext.GetApprenticeshipStartDateChangedEventBuilder()
            .WithStartDate(_startDateEarlierThanOriginal);

        _expectedNumberOfInstalments = _newExpectedNumberOfInstalmentsForEarlierStartDate;
    }

    [Given("the new start date is later than, and in the same academic year, as the current start date")]
    public void SetLaterStartDateChangeInSameAcademicYear()
    {
        _scenarioContext.GetApprenticeshipStartDateChangedEventBuilder()
            .WithStartDate(_startDateLaterThanOriginal);
        _expectedNumberOfInstalments = _newExpectedNumberOfInstalmentsForLaterStartDate;
    }

    [Given("the new start date is in the next academic year to the current start date")]
    public void SetLaterStartDateChangeInNextAcademicYear()
    {
        _scenarioContext.GetApprenticeshipStartDateChangedEventBuilder()
            .WithStartDate(_startDateInNextAcademicYearToOriginal);

        _expectedNumberOfInstalments = _newExpectedNumberOfInstalmentsForStartDateInNextAcademicYear;
    }

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

    [Given(@"the earnings for the apprenticeship are calculated")]
    public async Task PublishApprenticeshipCreatedEvent()
    {
        var apprenticeshipCreatedEvent = _scenarioContext.GetApprenticeshipCreatedEventBuilder().Build();
        var testdebug = JsonConvert.SerializeObject(apprenticeshipCreatedEvent);
        await _testContext.TestFunction.PublishEvent(apprenticeshipCreatedEvent);
        _scenarioContext.Set(apprenticeshipCreatedEvent);

        await WaitHelper.WaitForItAsync(async() => await EnsureApprenticeshipEntityCreated(), "Failed to publish create");
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

	[When("the start date change is approved")]
	public async Task PublishStartDateChangeEvents()
	{
        var apprenticeshipStartDateChangedEvent = _scenarioContext.GetApprenticeshipStartDateChangedEventBuilder().Build();
        var testdebug = JsonConvert.SerializeObject(apprenticeshipStartDateChangedEvent);
        await _testContext.TestFunction.PublishEvent(apprenticeshipStartDateChangedEvent);
        _scenarioContext.Set(apprenticeshipStartDateChangedEvent);

        await WaitHelper.WaitForItAsync(async () => await EnsureRecalculationHasHappened(), "Failed to publish start date change");
	}

    [When("a withdrawal was sent partway through the apprenticeship")]
    public async Task PublishWithdrawnEvent()
    {
        var apprenticeshipWithdrawnEvent = _scenarioContext.GetApprenticeshipWithdrawnEventBuilder()
            .WithLastDayOfLearning(new DateTime(2020, 08, 31))
            .Build();
        _expectedNumberOfInstalments = 12;

        await _testContext.TestFunction.PublishEvent(apprenticeshipWithdrawnEvent);
        _scenarioContext.Set(apprenticeshipWithdrawnEvent);

        await WaitHelper.WaitForItAsync(async () => await EnsureRecalculationHasHappened(), "Failed to publish withdrawal");
    }

    [When("a withdrawal was sent prior to completion of qualifying period")]
    public async Task PublishWithdrawnEventPriorToQualifyingPeriodCompletion()
    {
        var apprenticeshipWithdrawnEvent = _scenarioContext.GetApprenticeshipWithdrawnEventBuilder()
            .WithLastDayOfLearning(new DateTime(2019, 10, 4))
            .Build();
        _expectedNumberOfInstalments = 0;
        _expectedNumberOfAdditionalPayments = 0;

        await _testContext.TestFunction.PublishEvent(apprenticeshipWithdrawnEvent);
        _scenarioContext.Set(apprenticeshipWithdrawnEvent);

        await WaitHelper.WaitForItAsync(async () => await EnsureRecalculationHasHappened(), "Failed to publish withdrawal");
    }

    [When("a start date change is approved resulting in a duration of (.*) days")]
    public async Task ApproveStartDateChangeToMakeDuration(int days)
    {
        var duration = days - 1;

        var apprenticeshipStartDateChangedEvent = _scenarioContext.GetApprenticeshipStartDateChangedEventBuilder()
            .WithApprovedDate(_changeRequestDate)
            .WithStartDate(_endDate.AddDays(-duration))
            .Build();

        await _testContext.TestFunction.PublishEvent(apprenticeshipStartDateChangedEvent);
        _scenarioContext.Set(apprenticeshipStartDateChangedEvent);

        await WaitHelper.WaitForItAsync(async () => await EnsureRecalculationHasHappened(), "Failed to publish start date change");
    }

    #endregion

    #region Assert
    [Then("the earnings are recalculated based on the new price")]
    public void AssertEarningsRecalculated()
    {
        var expectedTotal = _newTrainingPrice + _newAssessmentPrice; //todo
        var apprenticeshipModel = _scenarioContext.Get<ApprenticeshipModel>();
        var currentEpisode = apprenticeshipModel!.GetCurrentEpisode(TestSystemClock.Instance());
        var actualTotal = currentEpisode.EarningsProfile.OnProgramTotal + currentEpisode.EarningsProfile.CompletionPayment;

        if (expectedTotal != actualTotal)
        {
            Assert.Fail($"Earnings not updated, Expected Total:{expectedTotal}, Actual Total:{actualTotal}");
        }
    }

    [Then("the new price is recorded")]
    public void AssetNewPriceIsRecorded()
    {
        var apprenticeshipModel = _scenarioContext.Get<ApprenticeshipModel>();
        var currentEpisode = apprenticeshipModel!.GetCurrentEpisode(TestSystemClock.Instance());
        currentEpisode.Prices.Count.Should().Be(2);
    }

    [Then("the earnings are recalculated based on the funding band maximum")]
    public void AssertEarningsRecalculatedBasedOnBandMaximum()
    {
        var apprenticeshipModel = _scenarioContext.Get<ApprenticeshipModel>();
        var currentEpisode = apprenticeshipModel!.GetCurrentEpisode(TestSystemClock.Instance());

        var expectedTotal = _fundingBandMaximum;
        var actualTotal = currentEpisode.EarningsProfile.OnProgramTotal + currentEpisode.EarningsProfile.CompletionPayment;

        if (expectedTotal != actualTotal)
        {
            Assert.Fail($"Earnings not updated correctly, Expected Total:{expectedTotal}, Actual Total:{actualTotal}");
        }
    }

    [Then("the history of old and new earnings is maintained")]
    public void AssertHistoryUpdated()
    {
        var apprenticeshipModel = _scenarioContext.Get<ApprenticeshipModel>();
        var currentEpisode = apprenticeshipModel!.GetCurrentEpisode(TestSystemClock.Instance());
        if (currentEpisode.EarningsProfileHistory == null || !currentEpisode.EarningsProfileHistory.Any())
        {
            Assert.Fail("No earning history created");
        }
    }

    [Then("the earnings prior to the effective-from date are 'frozen' and do not change as part of this calculation")]
    public void AssertEarningsFrozen()
    {
        var apprenticeshipModel = _scenarioContext.Get<ApprenticeshipModel>();
        var instalmentsToValidate = GetFrozenInstalments(apprenticeshipModel!);

        foreach(var instalment in instalmentsToValidate)
        {
            var expectedInstalment = _originalEarningsProfile.Instalments.FirstOrDefault(x => 
                x.AcademicYear == instalment.AcademicYear &&
                x.DeliveryPeriod == instalment.DeliveryPeriod);

            if (expectedInstalment == null)
            {
                Assert.Fail("Regenerated instalments do not match delivery dates of the original calculations");
                return;
            }

            if (expectedInstalment.Amount != instalment.Amount)
            {
                Assert.Fail($"Frozen amount should be £{expectedInstalment.Amount} but was £{instalment.Amount} for academicYear{instalment.AcademicYear} period:{instalment.DeliveryPeriod}");
            }
        }
    }

    [Then("the number of instalments is determined by the number of census dates passed between the effective-from date and the planned end date of the apprenticeship")]
    [Then("the number of instalments is determined by the number of census dates passed between the new start date and the planned end date of the apprenticeship")]
    [Then("the number of instalments is determined by the number of census dates passed between the start date and the withdrawal date")]
    [Then("the number of instalments is zero")]
    public void AssertNumberOfInstalments()
    {
        var apprenticeshipModel = _scenarioContext.Get<ApprenticeshipModel>();
        var currentEpisode = apprenticeshipModel!.GetCurrentEpisode(TestSystemClock.Instance());
        var numberOfInstalments = currentEpisode.EarningsProfile.Instalments.Count;

        if (numberOfInstalments != _expectedNumberOfInstalments)
        {
            Assert.Fail($"Expected {_expectedNumberOfInstalments} but found {numberOfInstalments}");
        }
    }

    [Then("the number of additional payments is zero")]
    public void AssertNumberOfAdditionalPayments()
    {
        var apprenticeshipModel = _scenarioContext.Get<ApprenticeshipModel>();
        var currentEpisode = apprenticeshipModel!.GetCurrentEpisode(TestSystemClock.Instance());
        var additionalPaymentsCount = currentEpisode.EarningsProfile.AdditionalPayments.Count;

        if (additionalPaymentsCount != _expectedNumberOfAdditionalPayments)
        {
            Assert.Fail($"Expected {_expectedNumberOfAdditionalPayments} but found {additionalPaymentsCount}");
        }
    }

    [Then("the amount of each instalment is determined as: newPriceLessCompletion - earningsBeforeTheEffectiveFromDate / numberOfInstalments")]
    public void AssertRecalculatedInstamentAmountsAfterPriceChange()
    {
        var apprenticeshipModel = _scenarioContext.Get<ApprenticeshipModel>();
        var currentEpisode = apprenticeshipModel!.GetCurrentEpisode(TestSystemClock.Instance());

        var frozenInstalments = GetFrozenInstalments(apprenticeshipModel!);
        var earningsBeforeTheEffectiveFromDate = frozenInstalments.Sum(x => x.Amount);

        var numberOfRecalculatedInstalments = currentEpisode.EarningsProfile.Instalments.Count - frozenInstalments.Count;
        var newPriceLessCompletion = currentEpisode.EarningsProfile.OnProgramTotal;

        var expectedMonthlyPrice = Math.Round((newPriceLessCompletion - earningsBeforeTheEffectiveFromDate) / numberOfRecalculatedInstalments, 5);

        var numberOfMatchingInstalments = currentEpisode.EarningsProfile.Instalments
            .Count(x => x.Amount == expectedMonthlyPrice);

        
        if (numberOfMatchingInstalments != numberOfRecalculatedInstalments)
        {
            Assert.Fail($"Expected to find {numberOfRecalculatedInstalments} instalments of £{expectedMonthlyPrice} but found {numberOfMatchingInstalments}");
        }
    }

    [Then("the amount of each instalment is determined as: totalPriceLessCompletion / newNumberOfInstalments")]
    public void AssertRecalculatedInstalmentAmountsAfterStartDateChange()
    {
        var apprenticeshipModel = _scenarioContext.Get<ApprenticeshipModel>();
        var currentEpisode = apprenticeshipModel!.GetCurrentEpisode(TestSystemClock.Instance());

        var numberOfRecalculatedInstalments = currentEpisode.EarningsProfile.Instalments.Count;
        var totalPriceLessCompletion = currentEpisode.EarningsProfile.OnProgramTotal;

        var expectedMonthlyPrice = Math.Round(totalPriceLessCompletion / numberOfRecalculatedInstalments, 5);

        var numberOfMatchingInstalments = currentEpisode.EarningsProfile.Instalments
            .Count(x => x.Amount == expectedMonthlyPrice);
        
        if (numberOfMatchingInstalments != numberOfRecalculatedInstalments)
        {
            Assert.Fail($"Expected to find {numberOfRecalculatedInstalments} instalments of £{expectedMonthlyPrice} but found {numberOfMatchingInstalments}");
        }
    }

    [Then("a new earnings profile id is set")]
    public void AssertEarningsProfileId()
    {
        var apprenticeshipModel = _scenarioContext.Get<ApprenticeshipModel>();
        var currentEpisode = apprenticeshipModel!.GetCurrentEpisode(TestSystemClock.Instance());

        Assert.That(currentEpisode.EarningsProfile.EarningsProfileId != Guid.Empty &&
                    currentEpisode.EarningsProfile.EarningsProfileId != _originalEarningsProfile.EarningsProfileId);
    }

    [Then("the earnings are recalculated based on the new start date")]
    public void AssertEarningsRecalculatedBasedOnNewStartDate()
    {
        //Left empty on purpose
    }

    [Then(@"the there are (.*) earnings")]
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

        var currentEpisode = apprenticeshipEntity!.GetCurrentEpisode(TestSystemClock.Instance());
        _originalEarningsProfile = currentEpisode.EarningsProfile;
        return true;
    }

    private async Task<ApprenticeshipModel> GetApprenticeshipEntity()
    {
        return await _testContext.SqlDatabase.GetApprenticeship(_scenarioContext.Get<ApprenticeshipCreatedEvent>().ApprenticeshipKey);
    }

    private async Task<bool> EnsureRecalculationHasHappened()
    {
        var apprenticeshipEntity = await GetApprenticeshipEntity();

        var currentEpisode = apprenticeshipEntity!.GetCurrentEpisode(TestSystemClock.Instance());
        if (!currentEpisode.EarningsProfileHistory.Any())
        {
            return false;
        }

        _scenarioContext.Set(apprenticeshipEntity);
        return true;
    }

    private List<InstalmentModel> GetFrozenInstalments(ApprenticeshipModel apprenticeshipEntity)
    {
        var fromYear = _effectiveFromDate.ToAcademicYear();
        var fromPeriod = _effectiveFromDate.ToDeliveryPeriod();

        var currentEpisode = apprenticeshipEntity!.GetCurrentEpisode(TestSystemClock.Instance());

        return currentEpisode.EarningsProfile.Instalments
            .Where(x => 
                (x.AcademicYear == fromYear && x.DeliveryPeriod < fromPeriod) ||
                x.AcademicYear < fromYear
             ).ToList();
    }

    #endregion
}