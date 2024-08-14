using AutoFixture;
using NUnit.Framework;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Apprenticeship;
using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Moq;
using SFA.DAS.Funding.ApprenticeshipEarnings.DataAccess.ReadModel;
using SFA.DAS.Funding.ApprenticeshipEarnings.DataAccess.Mappers;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Services;
using SFA.DAS.Funding.ApprenticeshipEarnings.DurableEntities.Models;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.DataAccess.UnitTests.Mappers;

[TestFixture]
public class WhenMappingApprenticeshipToEarningsReadModels
{
    private readonly Fixture _fixture = new();
    private readonly Mock<ISystemClockService> _systemClockService = new();

    [Test]
    public void ThenEarningsAreMappedCorrectly()
    {
        // Arrange
        var currentEpisodeModel = _fixture.Create<ApprenticeshipEpisodeModel>();
        currentEpisodeModel.Prices = new List<PriceModel>
        {
            new()
            {
                ActualStartDate = DateTime.UtcNow.AddMonths(-10),
                PlannedEndDate = DateTime.UtcNow.AddMonths(10),
            }
        };

        var apprenticeshipEntityModel = _fixture
            .Build<ApprenticeshipEntityModel>()
            .With(x => x.ApprenticeshipEpisodes, new List<ApprenticeshipEpisodeModel>{ currentEpisodeModel })
            .Create();

        var apprenticeship = new Apprenticeship(apprenticeshipEntityModel);

        var expectedEarnings = currentEpisodeModel.EarningsProfile?.Instalments.Select(x => new Earning
        {
            Id = Guid.NewGuid(),
            AcademicYear = x.AcademicYear,
            Amount = x.Amount,
            DeliveryPeriod = x.DeliveryPeriod,
            ApprenticeshipKey = apprenticeship.ApprenticeshipKey,
            ApprovalsApprenticeshipId = apprenticeship.ApprovalsApprenticeshipId,
            EmployerAccountId = currentEpisodeModel.EmployerAccountId,
            FundingType = currentEpisodeModel.FundingType,
            UKPRN = currentEpisodeModel.UKPRN,
            Uln = apprenticeship.Uln,
            ApprenticeshipEpisodeKey = currentEpisodeModel.ApprenticeshipEpisodeKey
        }).ToList();

        _systemClockService.Setup(x => x.UtcNow).Returns(DateTimeOffset.UtcNow);

        // Act
        var result = apprenticeship.ToEarningsReadModels(_systemClockService.Object)?.ToList();

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(expectedEarnings.Count);
        foreach (var expectedEarning in expectedEarnings)
        {
            result.Should().ContainEquivalentOf(expectedEarning, options => options.Excluding(x => x.Id));
        }
    }
}