using AutoFixture;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using SFA.DAS.Funding.ApprenticeshipEarnings.DataAccess.Entities;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Apprenticeship;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Services;
using SFA.DAS.Funding.ApprenticeshipEarnings.Types;
using SFA.DAS.Learning.Types;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.Domain.UnitTests.ApprenticeshipFunding;

[TestFixture]
public class WhenRecalculatingEarningsForStartDateChange
{
    private Fixture? _fixture;
    private Mock<ISystemClockService>? _mockSystemClockService;
    private Apprenticeship.Apprenticeship? _apprenticeship;
    private Apprenticeship.ApprenticeshipEpisode? _currentEpisode;
    private Guid _episodeKey;
    private List<LearningEpisodePrice> _prices;
    private int _ageAtStartOfLearning;

    [SetUp]
    public void Setup()
    {
        _fixture = new Fixture();
        _mockSystemClockService = new Mock<ISystemClockService>();
        _mockSystemClockService.Setup(x => x.UtcNow).Returns(new DateTimeOffset(new DateTime(2023, 11, 1)));

        var LearningEpisode = _fixture.Create<EpisodeModel>();
        var prices = _fixture.CreateMany<EpisodePriceModel>(3).ToList();
        prices[0].StartDate = new DateTime(2023, 2, 1);
        prices[0].EndDate = new DateTime(2023, 5, 1);
        prices[1].StartDate = new DateTime(2023, 5, 1);
        prices[1].EndDate = new DateTime(2023, 7, 1);
        prices[2].StartDate = new DateTime(2023, 7, 1);
        prices[2].EndDate = new DateTime(2024, 2, 1);
        LearningEpisode.Prices = prices;

        var apprenticeshipEntityModel = _fixture
            .Build<ApprenticeshipModel>()
            .With(x => x.Episodes, new List<EpisodeModel> { LearningEpisode })
            .Create();

        _apprenticeship = Apprenticeship.Apprenticeship.Get(apprenticeshipEntityModel);
        _currentEpisode = _apprenticeship.ApprenticeshipEpisodes.First();


        _episodeKey = _currentEpisode.ApprenticeshipEpisodeKey;
        _prices = new List<LearningEpisodePrice>
        {
            new LearningEpisodePrice
            {
                Key = _currentEpisode.Prices.Last().PriceKey,
                StartDate = new DateTime(2023, 1, 1),
                EndDate = new DateTime(2024, 1, 1)
            }
        };
        _ageAtStartOfLearning = 20;
    }

    [Test]
    public void ThenTheStartDateAndEndDateAreUpdated()
    {
        // Act
        _apprenticeship.UpdatePrices(_prices, _episodeKey, _ageAtStartOfLearning, _mockSystemClockService.Object);

        // Assert
        var updatedPrice = _currentEpisode.Prices.FirstOrDefault(p => p.PriceKey == _prices.First().Key);
        updatedPrice.Should().NotBeNull();
        updatedPrice!.StartDate.Should().Be(_prices.Min(x=>x.StartDate));
        updatedPrice.EndDate.Should().Be(_prices.Max(x => x.EndDate));
    }

    [Test]
    public void ThenTheAgeAtStartOfLearningIsUpdated()
    {
        // Act
        _apprenticeship.UpdatePrices(_prices, _episodeKey, _ageAtStartOfLearning, _mockSystemClockService.Object);

        // Assert
        _currentEpisode.AgeAtStartOfApprenticeship.Should().Be(_ageAtStartOfLearning);
    }

    [Test]
    public void ThenTheDeletedPricesAreRemoved()
    {
        // Act
        _apprenticeship.UpdatePrices(_prices, _episodeKey, _ageAtStartOfLearning, _mockSystemClockService.Object);

        // Assert
        _currentEpisode.Prices.Should().OnlyContain(p => _prices.Any(eventPrices => eventPrices.Key == p.PriceKey));
    }

    [Test]
    public void ThenAnEarningsRecalculatedEventIsAdded()
    {
        // Act
        _apprenticeship.UpdatePrices(_prices, _episodeKey, _ageAtStartOfLearning, _mockSystemClockService.Object);

        // Assert
        var events = _apprenticeship.FlushEvents().OfType<EarningsProfileUpdatedEvent>().ToList();
        events.Should().HaveCount(1);
    }
}