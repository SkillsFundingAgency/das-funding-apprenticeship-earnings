using System;
using System.Collections.Generic;
using System.Linq;
using AutoFixture;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Services;
using SFA.DAS.Funding.ApprenticeshipEarnings.DurableEntities.Models;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.Domain.UnitTests.ApprenticeshipEpisode;

[TestFixture]
public class WhenUpdateAgreedPrice
{
    private Fixture _fixture;
    private Apprenticeship.ApprenticeshipEpisode _apprenticeshipEpisode;
    private ApprenticeshipEpisodeModel _apprenticeshipEpisodeModel;
    private decimal _newAgreedPrice;
    private List<Guid> _deletedPriceKeys;
    private Guid _newPriceKey;
    private Mock<ISystemClockService> _clockService;
    private DateTime _expectedNow;
    private Guid _expectedClosedOffPriceKey;

    [SetUp]
    public void Setup()
    {
        _fixture = new Fixture();
        _clockService = new Mock<ISystemClockService>();
    }

    [Test]
    public void ThenTheAgreedPriceIsUpdated()
    {
        // Arrange
        PopulateTestVariables();
        BuildApprenticeshipEpisode();

        // Act
        _apprenticeshipEpisode.UpdateAgreedPrice(_clockService.Object, _newAgreedPrice, _deletedPriceKeys, _newPriceKey);

        // Assert
        var newPrice = _apprenticeshipEpisode.Prices.Find(p => p.PriceKey == _newPriceKey);
        newPrice.Should().NotBeNull();
        newPrice!.AgreedPrice.Should().Be(_newAgreedPrice);
        newPrice.ActualStartDate.Should().Be(_expectedNow.Date);
    }

    [Test]
    public void ThenTheDeletedPricesAreRemoved()
    {
        // Arrange
        PopulateTestVariables();
        BuildApprenticeshipEpisode();

        // Act
        _apprenticeshipEpisode.UpdateAgreedPrice(_clockService.Object, _newAgreedPrice, _deletedPriceKeys, _newPriceKey);

        // Assert
        _apprenticeshipEpisode.Prices.Should().NotContain(p => _deletedPriceKeys.Contains(p.PriceKey));
    }

    [Test]
    public void ThenTheExistingActivePriceIsClosedOff()
    {
        // Arrange
        PopulateTestVariables();
        BuildApprenticeshipEpisode();

        // Act
        _apprenticeshipEpisode.UpdateAgreedPrice(_clockService.Object, _newAgreedPrice, _deletedPriceKeys, _newPriceKey);

        // Assert
        var existingPrice = _apprenticeshipEpisode.Prices.Find(p => p.PriceKey != _newPriceKey);
        existingPrice.Should().NotBeNull();
        existingPrice!.PlannedEndDate.Should().Be(_expectedNow.Date.AddDays(-1));
    }

    private void BuildApprenticeshipEpisode()
    {
        _apprenticeshipEpisodeModel = _fixture.Create<ApprenticeshipEpisodeModel>();
        _apprenticeshipEpisodeModel.Prices = new List<PriceModel>
        {
            new PriceModel //to be deleted
            {
                PriceKey = _deletedPriceKeys.First()
            },
            new PriceModel //to be closed off
            {
                PriceKey = _expectedClosedOffPriceKey,
                ActualStartDate = _expectedNow.AddMonths(-7),
                PlannedEndDate = _expectedNow.AddMonths(6)
            },
        };
        _apprenticeshipEpisode = new Apprenticeship.ApprenticeshipEpisode(_apprenticeshipEpisodeModel);
    }

    private void PopulateTestVariables()
    {
        _newAgreedPrice = 1500m;
        _deletedPriceKeys = new List<Guid>{ Guid.NewGuid() };
        _newPriceKey = Guid.NewGuid();
        _expectedNow = DateTime.UtcNow.AddDays(-5);
        _clockService.Setup(x => x.UtcNow).Returns(_expectedNow);
        _expectedClosedOffPriceKey = Guid.NewGuid();
    }
}