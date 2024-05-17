using NServiceBus;
using NUnit.Framework;
using SFA.DAS.Apprenticeships.Enums;
using SFA.DAS.Apprenticeships.Types;
using SFA.DAS.Funding.ApprenticeshipEarnings.AcceptanceTests.Helpers;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Apprenticeship.Events;
using SFA.DAS.Funding.ApprenticeshipEarnings.DurableEntities;
using SFA.DAS.Funding.ApprenticeshipEarnings.DurableEntities.Models;
using SFA.DAS.Funding.ApprenticeshipEarnings.TestHelpers;
using FundingPlatform = SFA.DAS.Apprenticeships.Types.FundingPlatform;
using FundingType = SFA.DAS.Apprenticeships.Types.FundingType;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.AcceptanceTests.StepDefinitions;

[Binding]
public class RecalculateEarningsStepDefinitions
{
    private readonly ScenarioContext _scenarioContext;
    private readonly TestContext _testContext;
    private static IEndpointInstance? _endpointInstance;
    private readonly Random _random = new();

    private ApprenticeshipCreatedEvent? _apprenticeshipCreatedEvent;
    private PriceChangeApprovedEvent? _priceChangeApprovedEvent;
    private ApprenticeshipStartDateChangedEvent? _startDateChangedEvent;

    #region Test Values
    private readonly DateTime _dateOfBirth = new DateTime(2000, 1, 1);
    private readonly int _ageAtStartOfApprenticeship = 21;

    private readonly DateTime _startDate = new DateTime(2019, 09, 01);
    private readonly DateTime _endDate = new DateTime(2022, 1, 1);
    private int _expectedNumberOfInstalments = 28;

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


	private EarningsProfileEntityModel _originalEarningsProfile;
    private ApprenticeshipEntity? _updatedApprenticeshipEntity;

    #endregion

    public RecalculateEarningsStepDefinitions(ScenarioContext scenarioContext, TestContext testContext)
    {
        _scenarioContext = scenarioContext;
        _testContext = testContext;
    }

    [BeforeTestRun]
    public static async Task StartEndpoint()
    {
        _endpointInstance = await EndpointHelper
            .StartEndpoint("Test.Funding.ApprenticeshipEarnings", true, new[]
            {
                typeof(ApprenticeshipCreatedEvent), 
                typeof(PriceChangeApprovedEvent), 
                typeof(ApprenticeshipStartDateChangedEvent), 
                typeof(EarningsRecalculatedEvent)
            });
    }

    [AfterTestRun]
    public static async Task StopEndpoint()
    {
        await _endpointInstance!.Stop()
            .ConfigureAwait(false);
    }

    #region Arrange
    [Given("earnings have been calculated for an apprenticeship in the pilot")]
    [Given("new earnings are to be calculated following a price change")]
    [Given("new earnings are to be calculated following a start date change")]
    public void ApprenticeshipCreated()
    {
        //  Gets published in the when clause just before the price change request to allow for any 'And' clauses to be added
        _apprenticeshipCreatedEvent = new ApprenticeshipCreatedEvent
        {
            AgreedPrice = 15000,
            ActualStartDate = _startDate,
            ApprenticeshipKey = Guid.NewGuid(),
            EmployerAccountId = 114,
            FundingType = FundingType.Levy,
            PlannedEndDate = _endDate,
            UKPRN = 116,
            TrainingCode = "AbleSeafarer",
            FundingEmployerAccountId = null,
            Uln = _random.Next().ToString(),
            LegalEntityName = "MyTrawler",
            ApprovalsApprenticeshipId = 120,
            DateOfBirth = _dateOfBirth,
            FundingBandMaximum = _fundingBandMaximum,
            AgeAtStartOfApprenticeship = _ageAtStartOfApprenticeship,
            FundingPlatform = FundingPlatform.DAS
        };

        _priceChangeApprovedEvent = new PriceChangeApprovedEvent
        {
            ApprenticeshipKey = _apprenticeshipCreatedEvent.ApprenticeshipKey,
            ApprenticeshipId = 123,
            TrainingPrice = 16000,
            AssessmentPrice = 1500,
            EffectiveFromDate = _effectiveFromDate,
            ApprovedBy = ApprovedBy.Employer,
            ApprovedDate = _changeRequestDate,
            EmployerAccountId = _apprenticeshipCreatedEvent.EmployerAccountId,
            ProviderId = 123
        };

        _startDateChangedEvent = new ApprenticeshipStartDateChangedEvent
        {
            ApprenticeshipKey = _apprenticeshipCreatedEvent.ApprenticeshipKey,
            ApprenticeshipId = 123,
            ActualStartDate = _startDate,
            EmployerAccountId = _apprenticeshipCreatedEvent.EmployerAccountId,
            ProviderId = 123,
            ApprovedDate = _changeRequestDate,
            ProviderApprovedBy = "",
            EmployerApprovedBy = "",
            Initiator = ""
        };
    }

    [Given("the total price is below or at the funding band maximum")]
    public void SetTotalBelowBandMaximum()
    {
        _apprenticeshipCreatedEvent!.AgreedPrice = _originalPrice;
    }

    [Given("the price change request is for a new total price above the funding band maximum")]
    public void SetTotalAboveBandMaximum()
    {
        _priceChangeApprovedEvent!.TrainingPrice = _newTrainingPriceAboveBandMax;
        _priceChangeApprovedEvent!.AssessmentPrice = _newAssessmentPrice;
    }

    [Given("a price change request was sent before the end of R14 of the current academic year")]
    public void SetPriceChangeApprovedDate()
    {
        _priceChangeApprovedEvent!.ApprovedDate = _changeRequestDate;
    }

    [Given("the price change request is for a new total price up to or at the funding band maximum")]
    public void SetPriceChange()
    {
        _priceChangeApprovedEvent!.TrainingPrice = _newTrainingPrice;
        _priceChangeApprovedEvent!.AssessmentPrice = _newAssessmentPrice;
    }

    [Given("a start date change request was sent before the end of R14 of the current academic year")]
    public void SetStartDateChangeApprovedDate()
    {
	    _startDateChangedEvent!.ApprovedDate = _changeRequestDate;
    }

    [Given("the new start date is earlier than, and in the same academic year, as the current start date")]
    public void SetEarlierStartDateChange()
    {
	    _startDateChangedEvent!.ActualStartDate = _startDateEarlierThanOriginal;
        _expectedNumberOfInstalments = _newExpectedNumberOfInstalmentsForEarlierStartDate;
    }

    [Given("the new start date is later than, and in the same academic year, as the current start date")]
    public void SetLaterStartDateChangeInSameAcademicYear()
    {
        _startDateChangedEvent!.ActualStartDate = _startDateLaterThanOriginal;
        _expectedNumberOfInstalments = _newExpectedNumberOfInstalmentsForLaterStartDate;
    }

    [Given("the new start date is in the next academic year to the current start date")]
    public void SetLaterStartDateChangeInNextAcademicYear()
    {
        _startDateChangedEvent!.ActualStartDate = _startDateInNextAcademicYearToOriginal;
        _expectedNumberOfInstalments = _newExpectedNumberOfInstalmentsForStartDateInNextAcademicYear;
    } 

	#endregion

	#region Act
	[When("the price change is approved by the other party before the end of year")]
    [When("the earnings are calculated")]
    public async Task PublishEvents()
    {
        await _endpointInstance.Publish(_apprenticeshipCreatedEvent);
        await WaitHelper.WaitForItAsync(async() => await EnsureApprenticeshipEntityCreated(), "Failed to publish create");
        await _endpointInstance.Publish(_priceChangeApprovedEvent);
        await WaitHelper.WaitForItAsync(async () => await EnsureRecalculationHasHappened(), "Failed to publish priceChange");
    }

	[When("the start date change is approved")]
	public async Task PublishStartDateChangeEvents()
	{
		await _endpointInstance.Publish(_apprenticeshipCreatedEvent);
		await WaitHelper.WaitForItAsync(async () => await EnsureApprenticeshipEntityCreated(), "Failed to publish create");
		await _endpointInstance.Publish(_startDateChangedEvent);
		await WaitHelper.WaitForItAsync(async () => await EnsureRecalculationHasHappened(), "Failed to publish start date change");
	}

	#endregion

	#region Assert
	[Then("the earnings are recalculated based on the new price")]
    public void AssertEarningsRecalculated()
    {
        var expectedTotal = _newTrainingPrice + _newAssessmentPrice;
        var actualTotal = _updatedApprenticeshipEntity!.Model.EarningsProfile.AdjustedPrice + _updatedApprenticeshipEntity.Model.EarningsProfile.CompletionPayment;

        if (expectedTotal != actualTotal)
        {
            Assert.Fail("Earnings not updated");
        }
    }

    [Then("the earnings are recalculated based on the lower of: the new total price and the funding band maximum")]
    public void AssertEarningsRecalculatedBasedOnBandMaximum()
    {
        var expectedTotal = _fundingBandMaximum;
        var actualTotal = _updatedApprenticeshipEntity!.Model.EarningsProfile.AdjustedPrice + _updatedApprenticeshipEntity.Model.EarningsProfile.CompletionPayment;

        if (expectedTotal != actualTotal)
        {
            Assert.Fail("Earnings not updated correctly");
        }
    }

    [Then("the history of old and new earnings is maintained")]
    public void AssertHistoryUpdated()
    {
        if (_updatedApprenticeshipEntity!.Model.EarningsProfileHistory == null || !_updatedApprenticeshipEntity.Model.EarningsProfileHistory.Any())
        {
            Assert.Fail("No earning history created");
        }
    }

    [Then("the earnings prior to the effective-from date are 'frozen' and do not change as part of this calculation")]
    public void AssertEarningsFrozen()
    {
        var instalmentsToValidate = GetFrozenInstalments(_updatedApprenticeshipEntity!);

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
    public void AssertNumberOfInstalmentsForPriceChange()
    {
        var numberOfInstalments = _updatedApprenticeshipEntity!.Model.EarningsProfile.Instalments.Count;

        if (numberOfInstalments != _expectedNumberOfInstalments)
        {
            Assert.Fail($"Expected {_expectedNumberOfInstalments} but found {numberOfInstalments}");
        }
    }

    [Then("the number of instalments is determined by the number of census dates passed between the new start date and the planned end date of the apprenticeship")]
    public void AssertNumberOfInstalmentsForStartDateChange()
    {
        var numberOfInstalments = _updatedApprenticeshipEntity!.Model.EarningsProfile.Instalments.Count;

        if (numberOfInstalments != _expectedNumberOfInstalments)
        {
            Assert.Fail($"Expected {_expectedNumberOfInstalments} but found {numberOfInstalments}");
        }
    }

    [Then("the amount of each instalment is determined as: newPriceLessCompletion - earningsBeforeTheEffectiveFromDate / numberOfInstalments")]
    public void AssertRecalculatedInstamentAmountsAfterPriceChange()
    {
        var frozenInstalments = GetFrozenInstalments(_updatedApprenticeshipEntity!);
        var earningsBeforeTheEffectiveFromDate = frozenInstalments.Sum(x => x.Amount);

        var numberOfRecalculatedInstalments = _updatedApprenticeshipEntity!.Model.EarningsProfile.Instalments.Count - frozenInstalments.Count;
        var newPriceLessCompletion = _updatedApprenticeshipEntity.Model.EarningsProfile.AdjustedPrice;

        var expectedMonthlyPrice = Math.Round((newPriceLessCompletion - earningsBeforeTheEffectiveFromDate) / numberOfRecalculatedInstalments, 5);

        var numberOfMatchingInstalments = _updatedApprenticeshipEntity.Model.EarningsProfile.Instalments
            .Count(x => x.Amount == expectedMonthlyPrice);

        
        if (numberOfMatchingInstalments != numberOfRecalculatedInstalments)
        {
            Assert.Fail($"Expected to find {numberOfRecalculatedInstalments} instalments of £{expectedMonthlyPrice} but found {numberOfMatchingInstalments}");
        }
    }

    [Then("the amount of each instalment is determined as: totalPriceLessCompletion / newNumberOfInstalments")]
    public void AssertRecalculatedInstalmentAmountsAfterStartDateChange()
    {
        var numberOfRecalculatedInstalments = _updatedApprenticeshipEntity!.Model.EarningsProfile.Instalments.Count;
        var totalPriceLessCompletion = _updatedApprenticeshipEntity.Model.EarningsProfile.AdjustedPrice;

        var expectedMonthlyPrice = Math.Round(totalPriceLessCompletion / numberOfRecalculatedInstalments, 5);

        var numberOfMatchingInstalments = _updatedApprenticeshipEntity.Model.EarningsProfile.Instalments
            .Count(x => x.Amount == expectedMonthlyPrice);
        
        if (numberOfMatchingInstalments != numberOfRecalculatedInstalments)
        {
            Assert.Fail($"Expected to find {numberOfRecalculatedInstalments} instalments of £{expectedMonthlyPrice} but found {numberOfMatchingInstalments}");
        }
    }

    [Then("a new earnings profile id is set")]
    public void AssertEarningsProfileId()
    {
        Assert.That(_updatedApprenticeshipEntity!.Model.EarningsProfile.EarningsProfileId != Guid.Empty &&
                    _updatedApprenticeshipEntity!.Model.EarningsProfile.EarningsProfileId != _originalEarningsProfile.EarningsProfileId);
    }

    [Then("the earnings are recalculated based on the new start date")]
    public void AssertEarningsRecalculatedBasedOnNewStartDate()
    {
	    var expectedTotal = _originalPrice;
        //TODO: update
	    var actualTotal = _updatedApprenticeshipEntity!.Model.EarningsProfile.AdjustedPrice + _updatedApprenticeshipEntity.Model.EarningsProfile.CompletionPayment;

	    if (expectedTotal != actualTotal)
	    {
		    Assert.Fail("Earnings not updated");
	    }
    }

	private async Task<bool> EnsureApprenticeshipEntityCreated()
    {
        var apprenticeshipEntity = await GetApprenticeshipEntity();
        if (apprenticeshipEntity == null)
        {
            return false;
        }

        _originalEarningsProfile = apprenticeshipEntity.Model.EarningsProfile;
        return true;
    }

    private async Task<ApprenticeshipEntity> GetApprenticeshipEntity()
    {
        return await _testContext.TestFunction!.GetEntity(nameof(ApprenticeshipEntity), _apprenticeshipCreatedEvent!.ApprenticeshipKey.ToString());
    }

    private async Task<bool> EnsureRecalculationHasHappened()
    {
        var apprenticeshipEntity = await GetApprenticeshipEntity();

        if (apprenticeshipEntity.Model.EarningsProfileHistory == null)
        {
            return false;
        }

        _updatedApprenticeshipEntity = apprenticeshipEntity;
        return true;
    }

    private List<InstalmentEntityModel> GetFrozenInstalments(ApprenticeshipEntity apprenticeshipEntity)
    {
        var fromYear = _effectiveFromDate.ToAcademicYear();
        var fromPeriod = _effectiveFromDate.ToDeliveryPeriod();

        return apprenticeshipEntity.Model.EarningsProfile.Instalments
            .Where(x => 
                (x.AcademicYear == fromYear && x.DeliveryPeriod < fromPeriod) ||
                x.AcademicYear < fromYear
             ).ToList();
    }

    #endregion
}
