using AutoFixture;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Models.Apprenticeship;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Services;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.UnitTests.TestHelpers;
using SFA.DAS.Funding.ApprenticeshipEarnings.Types;
using System;
using System.Linq;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.Domain.UnitTests.ApprenticeshipFunding;

[TestFixture]
public class WhenGeneratingNewEarningsProfileVersion
{
    private readonly Fixture _fixture = new();
    private ApprenticeshipLearning _learning;
    private ApprenticeshipEpisode _episode;
    private Mock<ISystemClockService> _mockSystemClock;

    [SetUp]
    public void SetUp()
    {
        _mockSystemClock = new Mock<ISystemClockService>();
        _mockSystemClock.Setup(x => x.UtcNow).Returns(new DateTime(2024, 12, 24));

        _learning = _fixture.CreateLearningWithApprenticeship(new DateTime(2024, 1, 1), new DateTime(2024, 12, 31), 12000m);
        _episode = _learning.Episodes.First();
        _episode.CalculateOnProgram(_learning, _mockSystemClock.Object, string.Empty);

        _learning.FlushEvents();
        _episode.HasEarningsProfileVersionUpdate = false;
    }

    [Test]
    public void ThenANewVersionIsGeneratedWithoutChangingTheEarnings()
    {
        // Arrange
        var originalVersion = _episode.EarningsProfile!.Version;
        var originalInstalments = _episode.EarningsProfile.Instalments.Select(x => (x.AcademicYear, x.DeliveryPeriod, x.Amount)).ToList();

        // Act
        _episode.GenerateNewEarningsProfileVersion(_mockSystemClock.Object);

        // Assert
        _episode.EarningsProfile.Version.Should().NotBe(originalVersion);
        _episode.EarningsProfile.Instalments.Select(x => (x.AcademicYear, x.DeliveryPeriod, x.Amount)).Should().BeEquivalentTo(originalInstalments);
        _episode.HasEarningsProfileVersionUpdate.Should().BeTrue();
    }

    [Test]
    public void ThenTheEarningsHistoryIsMaintained()
    {
        // Act
        _episode.GenerateNewEarningsProfileVersion(_mockSystemClock.Object);

        // Assert
        _learning.FlushEvents().OfType<EarningsProfileUpdatedEvent>().Should().ContainSingle(x => !x.InitialGeneration);
    }

    [Test]
    public void ThenANewVersionIsNotGeneratedWhenOneHasAlreadyBeenGenerated()
    {
        // Arrange
        _episode.UpdateWithdrawalDate(new DateTime(2024, 3, 15), _mockSystemClock.Object);
        _learning.Calculate(_mockSystemClock.Object, string.Empty);
        var recalculatedVersion = _episode.EarningsProfile!.Version;

        // Act
        _episode.GenerateNewEarningsProfileVersion(_mockSystemClock.Object);

        // Assert
        _episode.HasEarningsProfileVersionUpdate.Should().BeTrue();
        _episode.EarningsProfile.Version.Should().Be(recalculatedVersion);
    }
}
