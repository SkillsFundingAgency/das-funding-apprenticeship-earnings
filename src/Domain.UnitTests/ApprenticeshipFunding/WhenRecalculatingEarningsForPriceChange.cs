using AutoFixture;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Extensions;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Models;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Models.Apprenticeship;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Services;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.UnitTests.TestHelpers;
using SFA.DAS.Funding.ApprenticeshipEarnings.Types;
using SFA.DAS.Learning.Types;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.Domain.UnitTests.ApprenticeshipFunding;

[TestFixture]
public class WhenRecalculatingEarningsForPriceChange
{
    private readonly Fixture _fixture;
    private readonly Mock<ISystemClockService> _mockSystemClock;
    private ApprenticeshipLearning? _learningBeforeUpdate; //represents the apprenticeship before the price change
    private ApprenticeshipLearning? _learningAfterUpdate; // represents the apprenticeship after the price change
    private ApprenticeshipEpisode _episode;
    private decimal _originalPrice;
    private decimal _updatedPrice;
    private Guid _episodeKey;
    private List<LearningEpisodePrice> _prices;

    public WhenRecalculatingEarningsForPriceChange()
    {
        _mockSystemClock = new Mock<ISystemClockService>();
        _mockSystemClock.Setup(x => x.UtcNow).Returns(new DateTime(2021, 8, 30));
        _fixture = new Fixture();
    }

    [SetUp]
    public void SetUp()
    {
        _originalPrice = _fixture.Create<decimal>();
        _updatedPrice = _fixture.Create<decimal>();
        _learningBeforeUpdate = _fixture.CreateLearningWithApprenticeship(new DateTime(2021, 1, 15), new DateTime(2021, 12, 31), _originalPrice);
        _learningBeforeUpdate.Calculate(_mockSystemClock.Object, string.Empty);
        _learningAfterUpdate = _fixture.CreateUpdatedApprenticeship(_learningBeforeUpdate, newPrice: _updatedPrice);

        _episodeKey = _learningBeforeUpdate.Episodes.First().EpisodeKey;
        _prices = new List<LearningEpisodePrice>
        {
            new()
            {
                Key = _learningBeforeUpdate.Episodes.First().Prices.First().PriceKey,
                StartDate = _learningBeforeUpdate.Episodes.First().Prices.First().StartDate,
                EndDate = _learningBeforeUpdate.Episodes.First().Prices.First().EndDate,
                TotalPrice = _updatedPrice
            }
        };
        
        _episode = _learningAfterUpdate.Episodes.First();

    }

    [Test]
    public void ThenTheAgreedPriceIsUpdated()
    {
        _episode.UpdatePrices(_prices);
        _learningAfterUpdate.Calculate(_mockSystemClock.Object, string.Empty, _episodeKey);
        var currentEpisode = _learningAfterUpdate.GetCurrentEpisode(_mockSystemClock.Object);
        currentEpisode.Prices.OrderBy(x => x.StartDate).Last().AgreedPrice.Should().Be(_updatedPrice);
    }

    [Test]
    public void ThenTheOnProgramTotalIsCalculated()
    {
        _episode.UpdatePrices(_prices);
        _learningAfterUpdate.Calculate(_mockSystemClock.Object, string.Empty, _episodeKey);
        var currentEpisode = _learningAfterUpdate.GetCurrentEpisode(_mockSystemClock.Object);
        currentEpisode.EarningsProfile.OnProgramTotal.Should().Be(_updatedPrice * .8m);
    }

    [Test]
    public void ThenTheCompletionAmountIsCalculated()
    {
        _episode.UpdatePrices(_prices);
        _learningAfterUpdate.Calculate(_mockSystemClock.Object, string.Empty, _episodeKey);
        var currentEpisode = _learningAfterUpdate.GetCurrentEpisode(_mockSystemClock.Object);
        currentEpisode.EarningsProfile.CompletionPayment.Should().Be(_updatedPrice * .2m);
    }

    [Test]
    public void ThenTheSumOfTheInstalmentsMatchTheOnProgramTotal()
    {
        _episode.UpdatePrices(_prices);
        _learningAfterUpdate.Calculate(_mockSystemClock.Object, string.Empty, _episodeKey);

        var currentEpisode = _learningAfterUpdate.GetCurrentEpisode(_mockSystemClock.Object);
        currentEpisode.EarningsProfile.Instalments.Count.Should().Be(12);
        var sum = Math.Round(currentEpisode.EarningsProfile.Instalments.Sum(x => x.Amount), 2);
        sum.Should().Be(currentEpisode.EarningsProfile.OnProgramTotal);
    }

    [Test]
    public void ThenEarningsRecalculatedEventIsCreated()
    {
        _episode.UpdatePrices(_prices);
        _learningAfterUpdate.Calculate(_mockSystemClock.Object, string.Empty, _episodeKey);

        var events = _learningAfterUpdate.FlushEvents();
        events.Should().ContainSingle(x => x.GetType() == typeof(EarningsProfileUpdatedEvent));
    }

    [Test]
    public void ThenTheEarningsProfileIdIsGenerated()
    {
        _episode.UpdatePrices(_prices);
        _learningAfterUpdate.Calculate(_mockSystemClock.Object, string.Empty, _episodeKey);
        var currentEpisode = _learningAfterUpdate.GetCurrentEpisode(_mockSystemClock.Object);
        currentEpisode.EarningsProfile.EarningsProfileId.Should().NotBeEmpty();
    }

    [Test]
    public void ThenIfPricesAreTheSameNoRecalculationOccurs()
    {
        _prices.First().TotalPrice = _originalPrice;
        _episode.UpdatePrices(_prices);
        _learningAfterUpdate.Calculate(_mockSystemClock.Object, string.Empty, _episodeKey);
        var events = _learningAfterUpdate.FlushEvents();
        events.Should().NotContain(x => x.GetType() == typeof(EarningsProfileUpdatedEvent));
    }
}