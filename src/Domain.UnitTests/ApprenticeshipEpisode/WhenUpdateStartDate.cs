using System;
using System.Collections.Generic;
using System.Linq;
using AutoFixture;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using SFA.DAS.Funding.ApprenticeshipEarnings.Command.ApproveStartDateChangeCommand;
using SFA.DAS.Funding.ApprenticeshipEarnings.DurableEntities.Models;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.Domain.UnitTests.ApprenticeshipEpisode;

[TestFixture]
public class WhenUpdateStartDate
{
    private Fixture? _fixture;
    private Apprenticeship.ApprenticeshipEpisode? _apprenticeshipEpisode;
    private ApprenticeshipEpisodeModel? _apprenticeshipEpisodeModel;
    private DateTime _newStartDate;
    private DateTime _newEndDate;
    private int _newAgeAtStart;
    private List<Guid>? _deletedPriceKeys;
    private Guid _changingPriceKey;

    [SetUp]
    public void Setup()
    {
        _fixture = new Fixture();
    }

    [Test]
    public void ThenTheDatesAreUpdated()
    {
        // Arrange
        BuildApprenticeshipEpisode();
        PopulateTestVariables();

        // Act
        _apprenticeshipEpisode.UpdateStartDate(_newStartDate, _newEndDate, _newAgeAtStart, _deletedPriceKeys, _changingPriceKey, Mock.Of<ILogger<ApproveStartDateChangeCommandHandler>>());

        // Assert
        var updatedPrice = _apprenticeshipEpisode.Prices.Find(p => p.PriceKey == _changingPriceKey);
        updatedPrice.Should().NotBeNull();
        updatedPrice!.ActualStartDate.Should().Be(_newStartDate);
        updatedPrice.PlannedEndDate.Should().Be(_newEndDate);
    }

    [Test]
    public void ThenTheAgeAtStartIsUpdated()
    {
        // Arrange
        BuildApprenticeshipEpisode();
        PopulateTestVariables();

        // Act
        _apprenticeshipEpisode.UpdateStartDate(_newStartDate, _newEndDate, _newAgeAtStart, _deletedPriceKeys, _changingPriceKey, Mock.Of<ILogger<ApproveStartDateChangeCommandHandler>>());

        // Assert
        _apprenticeshipEpisode.AgeAtStartOfApprenticeship.Should().Be(_newAgeAtStart);
    }

    [Test]
    public void ThenTheDeletedPricesAreRemoved()
    {
        // Arrange
        BuildApprenticeshipEpisode();
        PopulateTestVariables();

        // Act
        _apprenticeshipEpisode.UpdateStartDate(_newStartDate, _newEndDate, _newAgeAtStart, _deletedPriceKeys, _changingPriceKey, Mock.Of<ILogger<ApproveStartDateChangeCommandHandler>>());

        // Assert
        _apprenticeshipEpisode.Prices.Should().NotContain(p => _deletedPriceKeys.Contains(p.PriceKey));
    }

    private void BuildApprenticeshipEpisode()
    {
        _apprenticeshipEpisodeModel = _fixture.Create<ApprenticeshipEpisodeModel>();
        _apprenticeshipEpisode = new Apprenticeship.ApprenticeshipEpisode(_apprenticeshipEpisodeModel);
    }

    private void PopulateTestVariables()
    {
        _newStartDate = new DateTime(2023, 1, 1);
        _newEndDate = new DateTime(2024, 1, 1);
        _newAgeAtStart = 20;
        _deletedPriceKeys = _apprenticeshipEpisodeModel.Prices.Take(1).Select(p => p.PriceKey).ToList();
        _changingPriceKey = _apprenticeshipEpisodeModel.Prices.Last().PriceKey;
    }
}