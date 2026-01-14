using System;
using System.Linq;
using AutoFixture;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Apprenticeship;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Services;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.UnitTests.TestHelpers;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.Domain.UnitTests.ApprenticeshipFunding;

[TestFixture]
public class WhenRemovingEarningsFollowingWithdrawal
{
    private readonly Fixture _fixture = new();
    private Apprenticeship.Apprenticeship _apprenticeship;
    private Mock<ISystemClockService> _mockSystemClock;

    [SetUp]
    public void SetUp()
    {
        _mockSystemClock = new Mock<ISystemClockService>();
        _mockSystemClock.Setup(x => x.UtcNow).Returns(new DateTime(2024, 12, 24));

        var actualStartDate = new DateTime(2024, 1, 1);
        var plannedEndDate = new DateTime(2024, 12, 31);
        var agreedPrice = 12000m;

        _apprenticeship = _fixture.CreateApprenticeship(actualStartDate, plannedEndDate, agreedPrice);
        var episode = _apprenticeship.ApprenticeshipEpisodes.First();

        episode.CalculateOnProgram(_apprenticeship, _mockSystemClock.Object);
    }

    [Test]
    public void ThenInstalmentsBeforeLastDayOfLearningArePreserved()
    {
        // Arrange
        var withdrawalDate = new DateTime(2024, 3, 15);
        var currentEpisode = _apprenticeship.GetCurrentEpisode(_mockSystemClock.Object);

        // Act
        currentEpisode.UpdateWithdrawalDate(withdrawalDate, _mockSystemClock.Object);
        _apprenticeship.Calculate(_mockSystemClock.Object);

        // Assert
        currentEpisode.EarningsProfile.Instalments.Count.Should().Be(2);
    }

    [Test]
    public void ThenAdditionalPaymentsBeforeLastDayOfLearningArePreserved()
    {
        // Arrange
        var withdrawalDate = new DateTime(2024, 6, 15);
        var currentEpisode = _apprenticeship.GetCurrentEpisode(_mockSystemClock.Object);

        // Act
        currentEpisode.UpdateWithdrawalDate(withdrawalDate, _mockSystemClock.Object);
        _apprenticeship.Calculate(_mockSystemClock.Object);

        // Assert
        currentEpisode.EarningsProfile.AdditionalPayments.Count.Should().Be(2);
    }

    [Test]
    public void ThenInstalmentsAfterLastDayOfLearningAreRemoved()
    {
        // Arrange
        var lastDayOfLearning = new DateTime(2024, 3, 15);
        var currentEpisode = _apprenticeship.GetCurrentEpisode(_mockSystemClock.Object);

        // Act
        currentEpisode.UpdateWithdrawalDate(lastDayOfLearning, _mockSystemClock.Object);
        _apprenticeship.Calculate(_mockSystemClock.Object);

        // Assert
        currentEpisode.EarningsProfile.Instalments.Should().OnlyContain(x =>
            x.AcademicYear < 2324 ||
            (x.AcademicYear == 2324 && x.DeliveryPeriod <= 7));
    }

    [Test]
    public void ThenAdditionalPaymentsAfterLastDayOfLearningAreRemoved()
    {
        // Arrange
        var withdrawalDate = new DateTime(2024, 3, 15);
        var currentEpisode = _apprenticeship.GetCurrentEpisode(_mockSystemClock.Object);

        // Act
        currentEpisode.UpdateWithdrawalDate(withdrawalDate, _mockSystemClock.Object);
        _apprenticeship.Calculate(_mockSystemClock.Object);

        // Assert
        currentEpisode.EarningsProfile.AdditionalPayments.Should().OnlyContain(x =>
            x.AcademicYear < 2324 ||
            (x.AcademicYear == 2324 && x.DeliveryPeriod <= 7));
    }


    [Test]
    public void ThenInstalmentsArePreservedForTheLastMonthIfTheLearnerWasInLearningOnTheCensusDate()
    {
        // Arrange
        var withdrawalDate = new DateTime(2024, 3, 31);
        var currentEpisode = _apprenticeship.GetCurrentEpisode(_mockSystemClock.Object);

        // Act
        currentEpisode.UpdateWithdrawalDate(withdrawalDate, _mockSystemClock.Object);
        _apprenticeship.Calculate(_mockSystemClock.Object);

        // Assert
        currentEpisode.EarningsProfile.Instalments.Should().Contain(x =>
            x.AcademicYear == 2324 && x.DeliveryPeriod == 8);
    }


    [Test]
    public void ThenAdditionalPaymentsArePreservedForTheLastMonthIfTheLearnerWasInLearningOnTheCensusDate()
    {
        // Arrange
        var withdrawalDate = new DateTime(2024, 3, 31); 
        var currentEpisode = _apprenticeship.GetCurrentEpisode(_mockSystemClock.Object);

        // Act
        currentEpisode.UpdateWithdrawalDate(withdrawalDate, _mockSystemClock.Object);
        _apprenticeship.Calculate(_mockSystemClock.Object);

        // Assert
        currentEpisode.EarningsProfile.AdditionalPayments.Should().Contain(x =>
            x.AcademicYear == 2324 && x.DeliveryPeriod == 8);
    }

    [Test]
    public void ThenNewEarningsProfileIdIsGenerated()
    {
        // Arrange
        var withdrawalDate = new DateTime(2024, 6, 30);
        var currentEpisode = _apprenticeship.GetCurrentEpisode(_mockSystemClock.Object);

        // Act
        currentEpisode.UpdateWithdrawalDate(withdrawalDate, _mockSystemClock.Object);
        _apprenticeship.Calculate(_mockSystemClock.Object);

        // Assert
        currentEpisode.EarningsProfile.EarningsProfileId.Should().NotBeEmpty();
    }
}