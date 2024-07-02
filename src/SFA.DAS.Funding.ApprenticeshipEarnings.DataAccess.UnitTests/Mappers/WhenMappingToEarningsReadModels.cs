using AutoFixture;
using NUnit.Framework;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Apprenticeship;
using System;
using System.Linq;
using FluentAssertions;
using SFA.DAS.Funding.ApprenticeshipEarnings.DataAccess.ReadModel;
using SFA.DAS.Funding.ApprenticeshipEarnings.DataAccess.Mappers;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.DataAccess.UnitTests.Mappers;

[TestFixture]
public class WhenMappingApprenticeshipToEarningsReadModels
{
    private readonly Fixture _fixture = new();

    [Test]
    public void ThenEarningsAreMappedCorrectly()
    {
        // Arrange
        var apprenticeship = _fixture
            .Create<Apprenticeship>();

        var currentEpisode = apprenticeship.ApprenticeshipEpisodes.FirstOrDefault();
        var expectedEarnings = currentEpisode.EarningsProfile?.Instalments.Select(x => new Earning
        {
            Id = Guid.NewGuid(),
            AcademicYear = x.AcademicYear,
            Amount = x.Amount,
            DeliveryPeriod = x.DeliveryPeriod,
            ApprenticeshipKey = apprenticeship.ApprenticeshipKey,
            ApprovalsApprenticeshipId = apprenticeship.ApprovalsApprenticeshipId,
            EmployerAccountId = currentEpisode.EmployerAccountId,
            FundingType = currentEpisode.FundingType,
            UKPRN = currentEpisode.UKPRN,
            Uln = apprenticeship.Uln,
            ApprenticeshipEpisodeKey = currentEpisode.ApprenticeshipEpisodeKey
        }).ToList();

        // Act
        var result = apprenticeship.ToEarningsReadModels()?.ToList();

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(expectedEarnings.Count);
        foreach (var expectedEarning in expectedEarnings)
        {
            result.Should().ContainEquivalentOf(expectedEarning, options => options.Excluding(x => x.Id));
        }
    }
}