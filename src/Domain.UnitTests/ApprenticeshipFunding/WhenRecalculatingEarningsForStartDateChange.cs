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