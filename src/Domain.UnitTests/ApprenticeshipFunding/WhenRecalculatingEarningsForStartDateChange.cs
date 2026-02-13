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
    private const int _ageAtStartOfLearning = 20;

    [SetUp]
    public void Setup()
    {
        var startDate = new DateTime(2023, 2, 1);
        _fixture = new Fixture();
        _mockSystemClockService = new Mock<ISystemClockService>();
        _mockSystemClockService.Setup(x => x.UtcNow).Returns(new DateTimeOffset(new DateTime(2023, 11, 1)));

        var learningEpisode = _fixture.Create<EpisodeModel>();
        var prices = _fixture.CreateMany<EpisodePriceModel>(3).ToList();
        prices[0].StartDate = new DateTime(2023, 2, 1);
        prices[0].EndDate = new DateTime(2023, 5, 1);
        prices[1].StartDate = new DateTime(2023, 5, 1);
        prices[1].EndDate = new DateTime(2023, 7, 1);
        prices[2].StartDate = new DateTime(2023, 7, 1);
        prices[2].EndDate = new DateTime(2024, 2, 1);
        learningEpisode.Prices = prices;
        learningEpisode.WithdrawalDate = null;
        learningEpisode.CompletionDate = null;
        learningEpisode.FundingBandMaximum = int.MaxValue;
        learningEpisode.PeriodsInLearning = new List<EpisodePeriodInLearningModel>();
        learningEpisode.EarningsProfile.MathsAndEnglishCourses = new List<MathsAndEnglishModel>();

        var apprenticeshipEntityModel = _fixture
            .Build<LearningModel>()
            .With(x => x.DateOfBirth, startDate.AddYears(-_ageAtStartOfLearning))
            .With(x => x.Episodes, new List<EpisodeModel> { learningEpisode })
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
    }

    [Test]
    public void ThenTheStartDateAndEndDateAreUpdated()
    {
        // Act
        _currentEpisode.UpdatePrices(_prices);
        _apprenticeship.Calculate(_mockSystemClockService.Object, _episodeKey);

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
        _currentEpisode.UpdatePrices(_prices);
        _apprenticeship.Calculate(_mockSystemClockService.Object, _episodeKey);

        // Assert
        _currentEpisode.AgeAtStartOfApprenticeship.Should().Be(_ageAtStartOfLearning);
    }

    [Test]
    public void ThenTheDeletedPricesAreRemoved()
    {
        // Act
        _currentEpisode.UpdatePrices(_prices);
        _apprenticeship.Calculate(_mockSystemClockService.Object, _episodeKey);

        // Assert
        _currentEpisode.Prices.Should().OnlyContain(p => _prices.Any(eventPrices => eventPrices.Key == p.PriceKey));
    }

    [Test]
    public void ThenAnEarningsRecalculatedEventIsAdded()
    {
        // Act
        _currentEpisode.UpdatePrices(_prices);
        _apprenticeship.Calculate(_mockSystemClockService.Object, _episodeKey);

        // Assert
        var events = _apprenticeship.FlushEvents().OfType<EarningsProfileUpdatedEvent>().ToList();
        events.Should().HaveCount(1);
    }
}