using NUnit.Framework;
using SFA.DAS.Apprenticeships.Types;
using SFA.DAS.Funding.ApprenticeshipEarnings.AcceptanceTests.Extensions;
using SFA.DAS.Funding.ApprenticeshipEarnings.AcceptanceTests.Model;
using SFA.DAS.Funding.ApprenticeshipEarnings.Command.SaveCareDetailsCommand;
using SFA.DAS.Funding.ApprenticeshipEarnings.Command.SaveLearningSupportCommand;
using SFA.DAS.Funding.ApprenticeshipEarnings.DataAccess.Entities;
using SFA.DAS.Funding.ApprenticeshipEarnings.TestHelpers;
using SFA.DAS.Funding.ApprenticeshipEarnings.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using TechTalk.SpecFlow.Assist;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.AcceptanceTests.StepDefinitions;

[Binding]
public class AdditionalPaymentsStepDefinitions
{
    private readonly ScenarioContext _scenarioContext;
    private readonly TestContext _testContext;

    public AdditionalPaymentsStepDefinitions(ScenarioContext scenarioContext, TestContext testContext)
    {
        _scenarioContext = scenarioContext;
        _testContext = testContext;
    }

    [Given(@"the following learning support payment information is provided")]
    public async Task GivenTheFollowingLearningSupportPaymentInformationIsProvided(Table table)
    {
        var expected = table.CreateSet<LearningSupportPaymentDetail>().ToList();
        await _testContext.TestInnerApi.Patch($"/apprenticeship/{_scenarioContext.Get<ApprenticeshipCreatedEvent>().ApprenticeshipKey}/learningSupport", expected);
    }

    [When(@"care details are saved with (.*) (.*) (.*)")]
    [Given(@"care details are saved with (.*) (.*) (.*)")]
    public async Task SaveCareDetails(bool careLeaverEmployerConsentGiven, bool isCareLeaver, bool hasEHCP)
    {
        var request = new SaveCareDetailsRequest { CareLeaverEmployerConsentGiven = careLeaverEmployerConsentGiven, IsCareLeaver = isCareLeaver, HasEHCP = hasEHCP };
        var apprenticeshipCreatedEvent = _scenarioContext.Get<ApprenticeshipCreatedEvent>();
        var apprenticehipKey = apprenticeshipCreatedEvent.ApprenticeshipKey;
        await _testContext.TestInnerApi.Patch($"/apprenticeship/{apprenticehipKey}/careDetails", request);
    }

    [Then(@"recalculate event is sent with the following incentives")]
    public void ThenRecalculateEventIsSentWithTheFollowingIncentives(Table table)
    {
        var data = table.CreateSet<AdditionalPaymentExpectationModel>().ToList();
        var apprenticeshipCreatedEvent = _scenarioContext.Get<ApprenticeshipCreatedEvent>();
        var recalculateEvent = _testContext.MessageSession.ReceivedEvents<ApprenticeshipEarningsRecalculatedEvent>().SingleOrDefault(x => x.ApprenticeshipKey == apprenticeshipCreatedEvent.ApprenticeshipKey);

        foreach (var expectedAdditionalPayment in data)
        {
            recalculateEvent.DeliveryPeriods.Should()
                .Contain(x => x.LearningAmount == expectedAdditionalPayment.Amount
                && x.InstalmentType == expectedAdditionalPayment.Type);
        }

    }

    [Then(@"Additional Payments are persisted as follows")]
    public async Task ThenAdditionalPaymentsArePersistedAsFollows(Table table)
    {
        var data = table.CreateSet<AdditionalPaymentDbExpectationModel>().ToList();

        var apprenticeshipCreatedEvent = _scenarioContext.Get<ApprenticeshipCreatedEvent>();

        var updatedEntity = await _testContext.SqlDatabase.GetApprenticeship(apprenticeshipCreatedEvent.ApprenticeshipKey);

        var additionalPaymentsInDb = updatedEntity.Episodes.First().EarningsProfile.AdditionalPayments;

        additionalPaymentsInDb.Should().HaveCount(data.Count);

        foreach (var expectedAdditionalPayment in data)
        {
            additionalPaymentsInDb.Should()
                .Contain(x => x.Amount == expectedAdditionalPayment.Amount
                && x.DueDate == expectedAdditionalPayment.DueDate
                && x.AdditionalPaymentType == expectedAdditionalPayment.Type);
        }
    }

    [Then(@"an EarningsGeneratedEvent is raised with the following incentives as Delivery Periods")]
    public async Task ThenAdditionalPaymentsAreGeneratedWithTheFollowingIncentivesAsDeliveryPeriods(Table table)
    {
        await WaitHelper.WaitForIt(() =>
                _testContext.MessageSession.ReceivedEvents<EarningsGeneratedEvent>().Any(),
            "Failed to find any EarningsGeneratedEvent"
        );

        var expected = table.CreateSet<AdditionalPaymentExpectationModel>()
            .Select(e => new
            {
                e.Type,
                e.Amount,
                e.CalendarMonth,
                e.CalendarYear
            }).ToList();

        var allActualIncentives = _testContext.MessageSession.ReceivedEvents<EarningsGeneratedEvent>()
            .SelectMany(e => e.DeliveryPeriods)
            .Where(x => x.InstalmentType is "ProviderIncentive" or "EmployerIncentive")
            .Select(dp => new
            {
                Type = dp.InstalmentType,
                Amount = dp.LearningAmount,
                dp.CalendarMonth,
                CalendarYear = dp.CalenderYear
            }).ToList();

        allActualIncentives.Should().BeEquivalentTo(expected, options => options.IncludingFields());
    }

    [Then(@"no Additional Payments are persisted")]
    public async Task ThenNoAdditionalPaymentsArePersisted()
    {
        var apprenticeshipCreatedEvent = _scenarioContext.Get<ApprenticeshipCreatedEvent>();

        var updatedEntity = await _testContext.SqlDatabase.GetApprenticeship(apprenticeshipCreatedEvent.ApprenticeshipKey);

        updatedEntity.Episodes.First().EarningsProfile.AdditionalPayments.Should().BeEmpty();
    }

    [Then(@"an EarningsGeneratedEvent is raised with no incentives as Delivery Periods")]
    public async Task ThenAnEarningsGeneratedEventIsRaisedWithNoIncentivesAsDeliveryPeriods()
    {
        await WaitHelper.WaitForIt(() =>
                _testContext.MessageSession.ReceivedEvents<EarningsGeneratedEvent>().Any(),
            "Failed to find any EarningsGeneratedEvent"
        );

        var allActualIncentives = _testContext.MessageSession.ReceivedEvents<EarningsGeneratedEvent>()
            .SelectMany(e => e.DeliveryPeriods)
            .Where(x => x.InstalmentType is "ProviderIncentive" or "EmployerIncentive")
            .ToList();

        allActualIncentives.Should().BeEmpty();
    }

    [Then("a first incentive payment is generated")]
    public void AssertFirstIncentivePayment()
    {
        AssertIncentivePayment("ProviderIncentive", false, true);
        AssertIncentivePayment("EmployerIncentive", false, true);
    }

    [Then("no first incentive payment is generated")]
    public void AssertNoFirstIncentivePayment()
    {
        AssertIncentivePayment("ProviderIncentive", false, false);
        AssertIncentivePayment("EmployerIncentive", false, false);
    }

    [Then("a second incentive payment is generated")]
    public void AssertSecondIncentivePayment()
    {
        AssertIncentivePayment("ProviderIncentive", true, true);
        AssertIncentivePayment("EmployerIncentive", true, true);
    }

    [Then("no second incentive payment is generated")]
    public void AssertNoSecondIncentivePayment()
    {
        AssertIncentivePayment("ProviderIncentive", true, false);
        AssertIncentivePayment("EmployerIncentive", true, false);
    }

    private void AssertIncentivePayment(string type, bool second, bool expectedPayment)
    {
        var startDateChangedEvent = _scenarioContext.Get<ApprenticeshipStartDateChangedEvent>();
        var apprenticeshipModel = _scenarioContext.Get<ApprenticeshipModel>();
        var currentEpisode = apprenticeshipModel!.GetCurrentEpisode(TestSystemClock.Instance());

        var expectedPeriod = second
            ? startDateChangedEvent.StartDate.AddDays(364).ToAcademicYearAndPeriod()
            : startDateChangedEvent.StartDate.AddDays(89).ToAcademicYearAndPeriod();

        if (expectedPayment)
            currentEpisode.EarningsProfile.AdditionalPayments.Should().Contain(x =>
                x.AcademicYear == expectedPeriod.AcademicYear
                && x.DeliveryPeriod == expectedPeriod.Period
                && x.AdditionalPaymentType == type
                && x.Amount == 500);
        else
            currentEpisode.EarningsProfile.AdditionalPayments.Should().NotContain(x =>
                x.AcademicYear == expectedPeriod.AcademicYear
                && x.DeliveryPeriod == expectedPeriod.Period
                && x.AdditionalPaymentType == type
                && x.Amount == 500);
    }
}
