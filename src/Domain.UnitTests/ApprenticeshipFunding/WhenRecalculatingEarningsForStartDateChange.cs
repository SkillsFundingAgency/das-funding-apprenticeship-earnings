using System;
using System.Collections.Generic;
using System.Linq;
using AutoFixture;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using SFA.DAS.Apprenticeships.Types;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Apprenticeship.Events;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Services;
using SFA.DAS.Funding.ApprenticeshipEarnings.DurableEntities.Models;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.Domain.UnitTests.ApprenticeshipFunding;

[TestFixture]
public class WhenRecalculatingEarningsForStartDateChange
{
    private Fixture? _fixture;
    private Mock<ISystemClockService>? _mockSystemClockService;
    private Apprenticeship.Apprenticeship? _apprenticeship;
    private Apprenticeship.ApprenticeshipEpisode? _currentEpisode;
    private ApprenticeshipStartDateChangedEvent _apprenticeshipStartDateChangedEvent;

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
            .With(x => x.ApprenticeshipEpisodes, new List<ApprenticeshipEpisodeModel> { apprenticeshipEpisode })
            .Create();

        _apprenticeship = new Apprenticeship.Apprenticeship(apprenticeshipEntityModel);
        _currentEpisode = _apprenticeship.ApprenticeshipEpisodes.First();

        _apprenticeshipStartDateChangedEvent = new ApprenticeshipStartDateChangedEvent
        {
            Episode = new Apprenticeships.Types.ApprenticeshipEpisode
            {
                Key = _currentEpisode.ApprenticeshipEpisodeKey,
                Prices = new List<ApprenticeshipEpisodePrice>
                {
                    new ApprenticeshipEpisodePrice
                    {
                        Key = _currentEpisode.Prices.Last().PriceKey,
                        StartDate = new DateTime(2023, 1, 1),
                        EndDate = new DateTime(2024, 1, 1)
                    }
                }
            }
        };
    }

    [Test]
    public void ThenTheStartDateAndEndDateAreUpdated()
    {
        // Act
        _apprenticeship.RecalculateEarningsEpisodeUpdated(_apprenticeshipStartDateChangedEvent, _mockSystemClockService.Object);

        // Assert
        var updatedPrice = _currentEpisode.Prices.Find(p => p.PriceKey == _apprenticeshipStartDateChangedEvent.Episode.Prices.First().Key);
        updatedPrice.Should().NotBeNull();
        //updatedPrice!.ActualStartDate.Should().Be(newStartDate); todo assert this when it comes from event
        updatedPrice.PlannedEndDate.Should().Be(_apprenticeshipStartDateChangedEvent.Episode.Prices.First().EndDate);
    }

    [Test]
    public void ThenTheAgeAtStartOfApprenticeshipIsUpdated()
    {
        // Act
        _apprenticeship.RecalculateEarningsEpisodeUpdated(_apprenticeshipStartDateChangedEvent, _mockSystemClockService.Object);

        // Assert
        //_currentEpisode.AgeAtStartOfApprenticeship.Should().Be(); todo assert this when it comes from the event
    }

    [Test]
    public void ThenTheDeletedPricesAreRemoved()
    {
        // Act
        _apprenticeship.RecalculateEarningsEpisodeUpdated(_apprenticeshipStartDateChangedEvent, _mockSystemClockService.Object);

        // Assert
        _currentEpisode.Prices.Should().OnlyContain(p => _apprenticeshipStartDateChangedEvent.Episode.Prices.Any(eventPrices => eventPrices.Key == p.PriceKey));
    }

    [Test]
    public void ThenAnEarningsRecalculatedEventIsAdded()
    {
        // Act
        _apprenticeship.RecalculateEarningsEpisodeUpdated(_apprenticeshipStartDateChangedEvent, _mockSystemClockService.Object);

        // Assert
        var events = _apprenticeship.FlushEvents().OfType<EarningsRecalculatedEvent>().ToList();
        events.Should().HaveCount(1);
        events.First().Apprenticeship.Should().BeEquivalentTo(_apprenticeship);
    }
}