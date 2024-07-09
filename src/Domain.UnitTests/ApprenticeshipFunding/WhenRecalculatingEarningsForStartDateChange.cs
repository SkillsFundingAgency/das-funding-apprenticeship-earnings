using System;
using System.Collections.Generic;
using System.Linq;
using AutoFixture;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Apprenticeship.Events;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Services;
using SFA.DAS.Funding.ApprenticeshipEarnings.DurableEntities.Models;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.Domain.UnitTests.ApprenticeshipFunding;

[TestFixture]
public class WhenRecalculatingEarningsForStartDateChange
{
    private Fixture _fixture;
    private Mock<ISystemClockService> _mockSystemClockService;
    private Apprenticeship.Apprenticeship _apprenticeship;
    private Apprenticeship.ApprenticeshipEpisode _currentEpisode;

    [SetUp]
    public void Setup()
    {
        _fixture = new Fixture();
        _mockSystemClockService = new Mock<ISystemClockService>();
        _mockSystemClockService.Setup(x => x.UtcNow).Returns(new DateTimeOffset(new DateTime(2023, 11, 1)));

        var apprenticeshipEpisode = _fixture.Create<ApprenticeshipEpisodeModel>();
        var prices = _fixture.CreateMany<PriceModel>(3).ToList();
        prices[0].ActualStartDate = new DateTime(2023, 2, 1);
        prices[0].PlannedEndDate = new DateTime(2023, 5, 1);
        prices[1].ActualStartDate = new DateTime(2023, 5, 1);
        prices[1].PlannedEndDate = new DateTime(2023, 7, 1);
        prices[2].ActualStartDate = new DateTime(2023, 7, 1);
        prices[2].PlannedEndDate = new DateTime(2024, 2, 1);
        apprenticeshipEpisode.Prices = prices;

        var apprenticeshipEntityModel = _fixture
            .Build<ApprenticeshipEntityModel>()
            .With(x => x.ApprenticeshipEpisodes, new List<ApprenticeshipEpisodeModel>{ apprenticeshipEpisode })
            .Create();
        
        _apprenticeship = new Apprenticeship.Apprenticeship(apprenticeshipEntityModel);
        _currentEpisode = _apprenticeship.ApprenticeshipEpisodes.First();
    }

    [Test]
    public void ThenTheStartDateAndEndDateAreUpdated()
    {
        // Arrange
        var newStartDate = new DateTime(2023, 1, 1);
        var newEndDate = new DateTime(2024, 1, 1);
        var ageAtStart = 25;
        var deletedPriceKeys = new List<Guid> { _currentEpisode.Prices.First().PriceKey };
        var changingPriceKey = _currentEpisode.Prices.Last().PriceKey;

        // Act
        _apprenticeship.RecalculateEarningsStartDateChange(_mockSystemClockService.Object, newStartDate, newEndDate, ageAtStart, deletedPriceKeys, changingPriceKey);

        // Assert
        var updatedPrice = _currentEpisode.Prices.Find(p => p.PriceKey == changingPriceKey);
        updatedPrice.Should().NotBeNull();
        updatedPrice!.ActualStartDate.Should().Be(newStartDate);
        updatedPrice.PlannedEndDate.Should().Be(newEndDate);
    }

    [Test]
    public void ThenTheAgeAtStartOfApprenticeshipIsUpdated()
    {
        // Arrange
        var newStartDate = new DateTime(2023, 1, 1);
        var newEndDate = new DateTime(2024, 1, 1);
        var ageAtStart = 25;
        var deletedPriceKeys = new List<Guid> { _currentEpisode.Prices.First().PriceKey };
        var changingPriceKey = _currentEpisode.Prices.Last().PriceKey;

        // Act
        _apprenticeship.RecalculateEarningsStartDateChange(_mockSystemClockService.Object, newStartDate, newEndDate, ageAtStart, deletedPriceKeys, changingPriceKey);

        // Assert
        _currentEpisode.AgeAtStartOfApprenticeship.Should().Be(ageAtStart);
    }

    [Test]
    public void ThenTheDeletedPricesAreRemoved()
    {
        // Arrange
        var newStartDate = new DateTime(2023, 1, 1);
        var newEndDate = new DateTime(2024, 1, 1);
        var ageAtStart = 25;
        var deletedPriceKeys = _currentEpisode.Prices.Take(1).Select(p => p.PriceKey).ToList(); // Assume we delete the first price
        var changingPriceKey = _currentEpisode.Prices.Last().PriceKey;

        // Act
        _apprenticeship.RecalculateEarningsStartDateChange(_mockSystemClockService.Object, newStartDate, newEndDate, ageAtStart, deletedPriceKeys, changingPriceKey);

        // Assert
        _currentEpisode.Prices.Should().NotContain(p => deletedPriceKeys.Contains(p.PriceKey));
    }

    [Test]
    public void ThenAnEarningsRecalculatedEventIsAdded()
    {
        // Arrange
        var newStartDate = new DateTime(2023, 1, 1);
        var newEndDate = new DateTime(2024, 1, 1);
        var ageAtStart = 25;
        var deletedPriceKeys = new List<Guid> { _currentEpisode.Prices.First().PriceKey };
        var changingPriceKey = _currentEpisode.Prices.Last().PriceKey;

        // Act
        _apprenticeship.RecalculateEarningsStartDateChange(_mockSystemClockService.Object, newStartDate, newEndDate, ageAtStart, deletedPriceKeys, changingPriceKey);

        // Assert
        var events = _apprenticeship.FlushEvents().OfType<EarningsRecalculatedEvent>().ToList();
        events.Should().HaveCount(1);
        events.First().Apprenticeship.Should().BeEquivalentTo(_apprenticeship);
    }
}

//[TestFixture]
//public class WhenRecalculatingEarningsForStartDateChange 
//{
//    private Fixture _fixture;
//    private Mock<ISystemClockService> _mockSystemClock;
//    private Apprenticeship.Apprenticeship? _apprenticeshipBeforeStartDateChange; //represents the apprenticeship before the start date change
//    private Apprenticeship.Apprenticeship? _sut; // represents the apprenticeship after the start date change
//    private DateTime _updatedStartDate;
//    private int _updatedAgeAtApprenticeshipStart;
//    private DateTime _orginalStartDate = new DateTime(2021, 1, 15);
//    private DateTime _orginalEndDate = new DateTime(2021, 12, 31);

//    public WhenRecalculatingEarningsForStartDateChange()
//    {
//        _fixture = new Fixture();
//        _mockSystemClock = new Mock<ISystemClockService>();
//        _mockSystemClock.Setup(x => x.UtcNow).Returns(new DateTime(2021, 8, 30));
//    }

//    [SetUp]
//    public void SetUp()
//    {
//        _updatedStartDate = new DateTime(2021, 3, 15);
//        _updatedAgeAtApprenticeshipStart = _fixture.Create<int>();
//        _apprenticeshipBeforeStartDateChange = _fixture.CreateApprenticeship(_orginalStartDate, _orginalEndDate, _fixture.Create<decimal>());
//        _apprenticeshipBeforeStartDateChange.CalculateEarnings(_mockSystemClock.Object);
//        _sut = _fixture.CreateUpdatedApprenticeship(_apprenticeshipBeforeStartDateChange, newStartDate: _updatedStartDate);
//    }

//    [Test]
//    public void ThenTheActualStartDateAndAgeAreUpdated()
//    {
//        _sut!.RecalculateEarningsStartDateChange(_mockSystemClock.Object, _updatedStartDate, _orginalEndDate, _updatedAgeAtApprenticeshipStart, new List<Guid>(), Guid.Empty); //todo review this and other calls in this test class for new field logic
//        var currentEpisode = _sut.GetCurrentEpisode(_mockSystemClock.Object);
//        currentEpisode.Prices.Single().ActualStartDate.Should().Be(_updatedStartDate);
//        currentEpisode.AgeAtStartOfApprenticeship.Should().Be(_updatedAgeAtApprenticeshipStart);
//    }

//    [Test]
//    public void ThenTheEarningsProfileIsCalculated()
//    {
//        _sut!.RecalculateEarningsStartDateChange(_mockSystemClock.Object, _updatedStartDate, _orginalEndDate, _updatedAgeAtApprenticeshipStart, new List<Guid>(), Guid.Empty);

//        var currentEpisode = _sut!.GetCurrentEpisode(_mockSystemClock.Object);
//        var expectedEpisode = _apprenticeshipBeforeStartDateChange!.GetCurrentEpisode(_mockSystemClock.Object);

//        currentEpisode.EarningsProfile.OnProgramTotal.Should().Be(expectedEpisode.EarningsProfile.OnProgramTotal);
//        currentEpisode.EarningsProfile.CompletionPayment.Should().Be(expectedEpisode.EarningsProfile.CompletionPayment);
//        currentEpisode.EarningsProfile.EarningsProfileId.Should().NotBe(expectedEpisode.EarningsProfile.EarningsProfileId);
//        currentEpisode.EarningsProfile.Instalments.Count.Should().Be(10);
            
//        var sum = Math.Round(currentEpisode.EarningsProfile.Instalments.Sum(x => x.Amount),2);     
//        sum.Should().Be(currentEpisode.EarningsProfile.OnProgramTotal);
//    }

//    [Test]
//    public void ThenEarningsRecalculatedEventIsCreated()
//    {
//        _sut!.RecalculateEarningsStartDateChange(_mockSystemClock.Object, _updatedStartDate, _orginalEndDate, _updatedAgeAtApprenticeshipStart, new List<Guid>(), Guid.Empty);

//        var events = _sut.FlushEvents();
//        events.Should().ContainSingle(x => x.GetType() == typeof(EarningsRecalculatedEvent));
//    }
        
//    [Test]
//    public void ThenTheEarningsProfileIdIsGenerated()
//    {
//        _sut!.RecalculateEarningsStartDateChange(_mockSystemClock.Object, _updatedStartDate, _orginalEndDate, _updatedAgeAtApprenticeshipStart, new List<Guid>(), Guid.Empty);
//        var currentEpisode = _sut!.GetCurrentEpisode(_mockSystemClock.Object);
//        currentEpisode.EarningsProfile.EarningsProfileId.Should().NotBeEmpty();
//    }
//}