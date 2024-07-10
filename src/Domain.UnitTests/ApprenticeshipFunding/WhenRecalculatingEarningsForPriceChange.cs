﻿using AutoFixture;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Apprenticeship;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Apprenticeship.Events;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Services;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.UnitTests.TestHelpers;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.Domain.UnitTests.ApprenticeshipFunding;

[TestFixture]
public class WhenRecalculatingEarningsForPriceChange
{
    private readonly Fixture _fixture;
    private readonly Mock<ISystemClockService> _mockSystemClock;
    private Apprenticeship.Apprenticeship? _existingApprenticeship; //represents the apprenticeship before the price change
    private Apprenticeship.Apprenticeship? _sut; // represents the apprenticeship after the price change
    private decimal _originalPrice;
    private decimal _updatedPrice;

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
        _existingApprenticeship = _fixture.CreateApprenticeship(new DateTime(2021, 1, 15), new DateTime(2021, 12, 31), _originalPrice);
        _existingApprenticeship.CalculateEarnings(_mockSystemClock.Object);
        _sut = _fixture.CreateUpdatedApprenticeship(_existingApprenticeship, newPrice: _updatedPrice);
    }

    [Test]
    public void ThenTheAgreedPriceIsUpdated()
    {
        _sut!.RecalculateEarningsPriceChange(_mockSystemClock.Object, _updatedPrice, new DateTime(2021, 6, 15), new List<Guid>(), Guid.Empty);
        var currentEpisode = _sut.GetCurrentEpisode(_mockSystemClock.Object);
        currentEpisode.Prices.OrderBy(x => x.ActualStartDate).Last().AgreedPrice.Should().Be(_updatedPrice);
    }

    [Test]
    public void ThenTheOnProgramTotalIsCalculated()
    {
        _sut!.RecalculateEarningsPriceChange(_mockSystemClock.Object, _updatedPrice, new DateTime(2021, 6, 15), new List<Guid>(), Guid.Empty);
        var currentEpisode = _sut.GetCurrentEpisode(_mockSystemClock.Object);
        currentEpisode.EarningsProfile.OnProgramTotal.Should().Be(_updatedPrice * .8m);
    }

    [Test]
    public void ThenTheCompletionAmountIsCalculated()
    {
        _sut!.RecalculateEarningsPriceChange(_mockSystemClock.Object, _updatedPrice, new DateTime(2021, 6, 15), new List<Guid>(), Guid.Empty);
        var currentEpisode = _sut.GetCurrentEpisode(_mockSystemClock.Object);
        currentEpisode.EarningsProfile.CompletionPayment.Should().Be(_updatedPrice * .2m);
    }

    [Test]
    public void ThenTheSumOfTheInstalmentsMatchTheOnProgramTotal()
    {
        _sut!.RecalculateEarningsPriceChange(_mockSystemClock.Object, _updatedPrice, new DateTime(2021, 6, 15), new List<Guid>(), Guid.Empty);

        var currentEpisode = _sut.GetCurrentEpisode(_mockSystemClock.Object);
        currentEpisode.EarningsProfile.Instalments.Count.Should().Be(12);
        var sum = Math.Round(currentEpisode.EarningsProfile.Instalments.Sum(x => x.Amount),2);     
        sum.Should().Be(currentEpisode.EarningsProfile.OnProgramTotal);
    }

    [Test]
    public void ThenEarningsRecalculatedEventIsCreated()
    {
        _sut!.RecalculateEarningsPriceChange(_mockSystemClock.Object, _updatedPrice, new DateTime(2021, 6, 15), new List<Guid>(), Guid.Empty);

        var events = _sut.FlushEvents();
        events.Should().ContainSingle(x => x.GetType() == typeof(EarningsRecalculatedEvent));
    }
        
    [Test]
    public void ThenTheEarningsProfileIdIsGenerated()
    {
        _sut!.RecalculateEarningsPriceChange(_mockSystemClock.Object, _updatedPrice, new DateTime(2021, 6, 15), new List<Guid>(), Guid.Empty);
        var currentEpisode = _sut.GetCurrentEpisode(_mockSystemClock.Object);
        currentEpisode.EarningsProfile.EarningsProfileId.Should().NotBeEmpty();
    }

    [Test]
    public void ThenAnExceptionIsThrownIfNoEarningsProfileExistsForTheCurrentEpisode()
    {
        _sut = _fixture.CreateUpdatedApprenticeship(_existingApprenticeship, newPrice: _updatedPrice, null, true);
        FluentActions
            .Invoking(() => _sut.RecalculateEarningsPriceChange(_mockSystemClock.Object, _updatedPrice, new DateTime(2021, 6, 15), new List<Guid>(), Guid.Empty))
            .Should()
            .Throw<Exception>();
    }
}