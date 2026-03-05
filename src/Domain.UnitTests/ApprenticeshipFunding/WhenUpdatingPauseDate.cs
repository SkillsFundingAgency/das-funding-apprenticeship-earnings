using System;
using System.Linq;
using AutoFixture;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Extensions;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Models;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Models.Apprenticeship;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Services;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.UnitTests.TestHelpers;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.Domain.UnitTests.ApprenticeshipFunding;

[TestFixture]
internal class WhenUpdatingPauseDate
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

        var actualStartDate = new DateTime(2024, 1, 1);
        var plannedEndDate = new DateTime(2024, 12, 31);
        var agreedPrice = 12000m;

        _learning = _fixture.CreateLearningWithApprenticeship(actualStartDate, plannedEndDate, agreedPrice);
        _episode = _learning.Episodes.First();

        _episode.CalculateOnProgram(_learning, _mockSystemClock.Object, string.Empty);
    }

    [Test]
    public void ThenInstalmentsBeforeLastDayOfLearningArePreserved()
    {
        // Arrange
        var withdrawalDate = new DateTime(2024, 3, 15);

        // Act
        _episode.UpdatePause(withdrawalDate);
        _learning.Calculate(_mockSystemClock.Object, string.Empty);

        // Assert
        var currentEpisode = _learning.GetCurrentEpisode(_mockSystemClock.Object);
        currentEpisode.EarningsProfile.Instalments.Count.Should().Be(2);
    }

    [Test]
    public void ThenAdditionalPaymentsBeforeLastDayOfLearningArePreserved()
    {
        // Arrange
        var withdrawalDate = new DateTime(2024, 6, 15);

        // Act
        _episode.UpdatePause(withdrawalDate);
        _learning.Calculate(_mockSystemClock.Object, string.Empty);

        // Assert
        var currentEpisode = _learning.GetCurrentEpisode(_mockSystemClock.Object);
        currentEpisode.EarningsProfile.AdditionalPayments.Count.Should().Be(2);
    }

    [Test]
    public void ThenInstalmentsAfterLastDayOfLearningAreRemoved()
    {
        // Arrange
        var lastDayOfLearning = new DateTime(2024, 3, 15);

        // Act
        _episode.UpdatePause(lastDayOfLearning);
        _learning.Calculate(_mockSystemClock.Object, string.Empty);

        // Assert
        var currentEpisode = _learning.GetCurrentEpisode(_mockSystemClock.Object);
        currentEpisode.EarningsProfile.Instalments.Should().OnlyContain(x =>
            x.AcademicYear < 2324 ||
            (x.AcademicYear == 2324 && x.DeliveryPeriod <= 7));
    }

    [Test]
    public void ThenAdditionalPaymentsAfterLastDayOfLearningAreRemoved()
    {
        // Arrange
        var withdrawalDate = new DateTime(2024, 3, 15);

        // Act
        _episode.UpdatePause(withdrawalDate);
        _learning.Calculate(_mockSystemClock.Object, string.Empty);

        // Assert
        var currentEpisode = _learning.GetCurrentEpisode(_mockSystemClock.Object);
        currentEpisode.EarningsProfile.AdditionalPayments.Should().OnlyContain(x =>
            x.AcademicYear < 2324 ||
            (x.AcademicYear == 2324 && x.DeliveryPeriod <= 7));
    }


    [Test]
    public void ThenInstalmentsArePreservedForTheLastMonthIfTheLearnerWasInLearningOnTheCensusDate()
    {
        // Arrange
        var withdrawalDate = new DateTime(2024, 3, 31);

        // Act
        _episode.UpdatePause(withdrawalDate);
        _learning.Calculate(_mockSystemClock.Object, string.Empty);

        // Assert
        var currentEpisode = _learning.GetCurrentEpisode(_mockSystemClock.Object);
        currentEpisode.EarningsProfile.Instalments.Should().Contain(x =>
            x.AcademicYear == 2324 && x.DeliveryPeriod == 8);
    }

    [Test]
    public void ThenAdditionalPaymentsArePreservedForTheLastMonthIfTheLearnerWasInLearningOnTheCensusDate()
    {
        // Arrange
        var withdrawalDate = new DateTime(2024, 3, 31);

        // Act
        _episode.UpdatePause(withdrawalDate);
        _learning.Calculate(_mockSystemClock.Object, string.Empty);

        // Assert
        var currentEpisode = _learning.GetCurrentEpisode(_mockSystemClock.Object);
        currentEpisode.EarningsProfile.AdditionalPayments.Should().Contain(x =>
            x.AcademicYear == 2324 && x.DeliveryPeriod == 8);
    }

    [Test]
    public void AndSettingItBackToNullThenAllPaymentsAreRestored()
    {
        // Arrange
        var numberOfInstalments = _learning.GetCurrentEpisode(_mockSystemClock.Object).EarningsProfile.Instalments.Count;
        var numberOfAdditionalPayments = _learning.GetCurrentEpisode(_mockSystemClock.Object).EarningsProfile.AdditionalPayments.Count;
        var lastDayOfLearning = new DateTime(2024, 3, 15);
        _episode.UpdatePause(lastDayOfLearning);
        _learning.Calculate(_mockSystemClock.Object, string.Empty);

        _episode.UpdatePause(null);
        _learning.Calculate(_mockSystemClock.Object, string.Empty);

        // Assert
        var currentEpisode = _learning.GetCurrentEpisode(_mockSystemClock.Object);
        currentEpisode.EarningsProfile.Instalments.Count.Should().Be(numberOfInstalments);
        currentEpisode.EarningsProfile.AdditionalPayments.Count.Should().Be(numberOfAdditionalPayments);
    }

    [Test]
    public void ThenNewEarningsProfileIdIsGenerated()
    {
        // Arrange
        var withdrawalDate = new DateTime(2024, 6, 30);

        // Act
        _episode.UpdatePause(withdrawalDate);
        _learning.Calculate(_mockSystemClock.Object, string.Empty);

        // Assert
        var currentEpisode = _learning.GetCurrentEpisode(_mockSystemClock.Object);
        currentEpisode.EarningsProfile.EarningsProfileId.Should().NotBeEmpty();
    }

}
