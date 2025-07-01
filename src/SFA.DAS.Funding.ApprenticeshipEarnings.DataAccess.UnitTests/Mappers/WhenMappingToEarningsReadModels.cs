using AutoFixture;
using NUnit.Framework;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Apprenticeship;
using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Moq;
using SFA.DAS.Funding.ApprenticeshipEarnings.DataAccess.Entities;
using SFA.DAS.Funding.ApprenticeshipEarnings.DataAccess.ReadModel;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Mappers;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Services;

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
        var currentEpisodeModel = _fixture.Create<EpisodeModel>();
        currentEpisodeModel.Prices = new List<EpisodePriceModel>
        {
            new()
            {
                StartDate = DateTime.UtcNow.AddMonths(-10),
                EndDate = DateTime.UtcNow.AddMonths(10),
            }
        };

        var apprenticeshipEntityModel = _fixture
            .Build<ApprenticeshipModel>()
            .With(x => x.Episodes, new List<EpisodeModel>{ currentEpisodeModel })
            .Create();

        var apprenticeship = Apprenticeship.Get(apprenticeshipEntityModel);

        var expectedEarnings = currentEpisodeModel.EarningsProfile?.Instalments.Select(x => new Earning
        {
            Id = Guid.NewGuid(),
            AcademicYear = x.AcademicYear,
            Amount = x.Amount,
            DeliveryPeriod = x.DeliveryPeriod,
            LearningKey = apprenticeship.LearningKey,
            ApprovalsApprenticeshipId = apprenticeship.ApprovalsApprenticeshipId,
            EmployerAccountId = currentEpisodeModel.EmployerAccountId,
            FundingType = currentEpisodeModel.FundingType,
            UKPRN = currentEpisodeModel.Ukprn,
            Uln = apprenticeship.Uln,
            LearningEpisodeKey = currentEpisodeModel.Key,
            IsNonLevyFullyFunded = apprenticeship.GetCurrentEpisode(_systemClockService.Object).IsNonLevyFullyFunded,
            FundingEmployerAccountId = currentEpisodeModel.FundingEmployerAccountId
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