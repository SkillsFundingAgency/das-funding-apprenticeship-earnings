using DurableTask.Core;
using Microsoft.Azure.Amqp.Framing;
using NServiceBus;
using NUnit.Framework;
using SFA.DAS.Apprenticeships.Types;
using SFA.DAS.Funding.ApprenticeshipEarnings.AcceptanceTests.Handlers;
using SFA.DAS.Funding.ApprenticeshipEarnings.AcceptanceTests.Helpers;
using SFA.DAS.Funding.ApprenticeshipEarnings.Command.PriceChangeApprovedCommand;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Apprenticeship.Events;
using SFA.DAS.Funding.ApprenticeshipEarnings.DurableEntities;
using SFA.DAS.Funding.ApprenticeshipEarnings.DurableEntities.Models;
using SFA.DAS.Funding.ApprenticeshipEarnings.TestHelpers;
using SFA.DAS.Funding.ApprenticeshipEarnings.Types;

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

    #region Test Values
    private readonly DateTime _dateOfBirth = new DateTime(2000, 1, 1);
    private readonly int _ageAtStartOfApprenticeship = 21;

    private readonly DateTime _startDate = new DateTime(2019, 09, 01);
    private readonly DateTime _endDate = new DateTime(2021, 1, 1);
    private readonly int _expectedNumberOfInstalments = 16;

    private readonly DateTime _priceChangeRequestDate = new DateTime(2020, 1, 1);
    private readonly DateTime _effectiveFromDate = new DateTime(2020, 2, 1);

    private readonly int _orginalPrice = 15000;
    private readonly int _fundingBandMaximum = 25000;

    private readonly int _newTrainingPrice = 17000;
    private readonly int _newAssessmentPrice = 3000;
    private readonly int _newTrainingPriceAboveBandMax = 26000;

    private List<InstalmentEntityModel>? _instalmentsBeforePriceChange;
    private ApprenticeshipEntity _updatedApprenticeshipEntity;

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
            .StartEndpoint("Test.Funding.ApprenticeshipEarnings", true, new[] { typeof(ApprenticeshipCreatedEvent), typeof(PriceChangeApprovedEvent), typeof(EarningsRecalculatedEvent) });
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
    public void EventCreated()
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
            ApprovedDate = _priceChangeRequestDate,
            EmployerAccountId = _apprenticeshipCreatedEvent.EmployerAccountId,
            ProviderId = 123
        };
    }

    [Given("the total price is below or at the funding band maximum")]
    public void SetTotalBelowBandMaximum()
    {
        _apprenticeshipCreatedEvent!.AgreedPrice = _orginalPrice;
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
        _priceChangeApprovedEvent!.ApprovedDate = _priceChangeRequestDate;
    }

    [Given("the price change request is for a new total price up to or at the funding band maximum")]
    public void SetPriceChange()
    {
        _priceChangeApprovedEvent!.TrainingPrice = _newTrainingPrice;
        _priceChangeApprovedEvent!.AssessmentPrice = _newAssessmentPrice;
    }

    #endregion

    #region Act
    [When("the change is approved by the other party before the end of year")]
    [When("the earnings are calculated")]
    public async Task PublishEvents()
    {
        await _endpointInstance.Publish(_apprenticeshipCreatedEvent);
        await WaitHelper.WaitForItAsync(async() => await EnsureApprenticeshipEntityCreated(), "Failed to publish create");
        await _endpointInstance.Publish(_priceChangeApprovedEvent);
        await WaitHelper.WaitForItAsync(async () => await EnsureRecalulationHasHappened(), "Failed to publish priceChange");
    }

    #endregion

    #region Assert
    [Then("the earnings are recalculated based on the new price")]
    public void AssertEarningsRecalculated()
    {
        var expectedTotal = _newTrainingPrice + _newAssessmentPrice;
        var actualTotal = _updatedApprenticeshipEntity.Model.EarningsProfile.AdjustedPrice + _updatedApprenticeshipEntity.Model.EarningsProfile.CompletionPayment;

        if (expectedTotal != actualTotal)
        {
            Assert.Fail("Earnings not updated");
        }
    }

    [Then("the earnings are recalculated based on the lower of: the new total price and the funding band maximum")]
    public void AssertEarningsRecalculatedBasedOnBandMaximum()
    {
        var expectedTotal = _fundingBandMaximum;
        var actualTotal = _updatedApprenticeshipEntity.Model.EarningsProfile.AdjustedPrice + _updatedApprenticeshipEntity.Model.EarningsProfile.CompletionPayment;

        if (expectedTotal != actualTotal)
        {
            Assert.Fail("Earnings not updated correctly");
        }
    }

    [Then("the history of old and new earnings is maintained")]
    public void AssertHistoryUpdated()
    {
        if (_updatedApprenticeshipEntity.Model.EarningsProfileHistory == null || !_updatedApprenticeshipEntity.Model.EarningsProfileHistory.Any())
        {
            Assert.Fail("No earning history created");
        }
    }

    [Then("the earnings prior to the effective-from date are 'frozen' and do not change as part of this calculation")]
    public void AssertEarningsFrozen()
    {
        var instalmentsToValidate = GetFrozenInstalments(_updatedApprenticeshipEntity);

        foreach(var instalment in instalmentsToValidate)
        {
            var expectedInstalment = _instalmentsBeforePriceChange!.FirstOrDefault(x => 
                x.AcademicYear == instalment.AcademicYear &&
                x.DeliveryPeriod == instalment.DeliveryPeriod);

            if (expectedInstalment == null)
            {
                Assert.Fail("Regenerated instalments do not match delivery dates of the orginal calculations");
                return;
            }

            if (expectedInstalment.Amount != instalment.Amount)
            {
                Assert.Fail($"Frozen amount should be £{expectedInstalment.Amount} but was £{instalment.Amount} for academicYear{instalment.AcademicYear} period:{instalment.DeliveryPeriod}");
            }
        }
    }

    [Then("the number of instalments is determined by the number of census dates passed between the effective-from date and the planned end date of the apprenticeship")]
    public void AssertNumberOfInstalments()
    {
        var numberOfInstalments = _updatedApprenticeshipEntity.Model.EarningsProfile.Instalments.Count;

        if (numberOfInstalments != _expectedNumberOfInstalments)
        {
            Assert.Fail($"Expected {_expectedNumberOfInstalments} but found {numberOfInstalments}");
        }
    }

    [Then("the amount of each instalment is determined as: newPriceLessCompletion - earningsBeforeTheEffectiveFromDate / numberOfInstalments")]
    public void AssertRecalculatedInstamentAmounts()
    {
        var frozenInstalments = GetFrozenInstalments(_updatedApprenticeshipEntity);
        var earningsBeforeTheEffectiveFromDate = frozenInstalments.Sum(x => x.Amount);

        var numberOfRecalculatedInstalments = _updatedApprenticeshipEntity.Model.EarningsProfile.Instalments.Count - frozenInstalments.Count;
        var newPriceLessCompletion = _updatedApprenticeshipEntity.Model.EarningsProfile.AdjustedPrice;

        var expectedMonthlyPrice = Math.Round((newPriceLessCompletion - earningsBeforeTheEffectiveFromDate) / numberOfRecalculatedInstalments, 5);

        var numberOfMatchingInstalments = _updatedApprenticeshipEntity.Model.EarningsProfile.Instalments
            .Where(x => x.Amount == expectedMonthlyPrice)
            .Count();

        
        if (numberOfMatchingInstalments != numberOfRecalculatedInstalments)
        {
            Assert.Fail($"Expected to find {numberOfRecalculatedInstalments} instalments of £{expectedMonthlyPrice} but found {numberOfMatchingInstalments}");
        }
    }

    private async Task<bool> EnsureApprenticeshipEntityCreated()
    {
        var apprenticeshipEntity = await GetApprenticeshipEntity();
        if (apprenticeshipEntity == null)
        {
            return false;
        }

        _instalmentsBeforePriceChange = apprenticeshipEntity.Model.EarningsProfile.Instalments;
        return true;
    }

    private async Task<ApprenticeshipEntity> GetApprenticeshipEntity()
    {
        return await _testContext.TestFunction!.GetEntity(nameof(ApprenticeshipEntity), _apprenticeshipCreatedEvent.ApprenticeshipKey.ToString());
    }

    private async Task<bool> EnsureRecalulationHasHappened()
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
        return apprenticeshipEntity.Model.EarningsProfile.Instalments
            .Where(x => 
                x.AcademicYear <= _effectiveFromDate.ToAcademicYear() && 
                x.DeliveryPeriod < _effectiveFromDate.ToDeliveryPeriod())
            .ToList();
    }

    #endregion
}
