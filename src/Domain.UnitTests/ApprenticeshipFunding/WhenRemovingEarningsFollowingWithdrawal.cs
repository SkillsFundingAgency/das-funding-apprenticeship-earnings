using System;
using System.Linq;
using AutoFixture;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Apprenticeship;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Apprenticeship.Events;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Services;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.UnitTests.TestHelpers;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.Domain.UnitTests.ApprenticeshipFunding;

[TestFixture]
public class WhenRemovingEarningsFollowingWithdrawal
{
    private readonly Fixture _fixture = new();
    private Apprenticeship.Apprenticeship _sut;
    private Mock<ISystemClockService> _mockSystemClock;

    [SetUp]
    public void SetUp()
    {
        _mockSystemClock = new Mock<ISystemClockService>();
        _mockSystemClock.Setup(x => x.UtcNow).Returns(new DateTime(2024, 12, 24));

        var actualStartDate = new DateTime(2024, 1, 1);
        var plannedEndDate = new DateTime(2024, 12, 31);
        var agreedPrice = 12000m;

        _sut = _fixture.CreateApprenticeship(actualStartDate, plannedEndDate, agreedPrice);
        var episode = _sut.ApprenticeshipEpisodes.First();

        episode.CalculateEpisodeEarnings(_sut, _mockSystemClock.Object);
    }

    [Test]
    public void ThenInstalmentsBeforeLastDayOfLearningArePreserved()
    {
        // Arrange
        var lastDayOfLearning = new DateTime(2024, 3, 15);

        // Act
        _sut.RemovalEarningsFollowingWithdrawal(lastDayOfLearning, _mockSystemClock.Object);

        // Assert
        var currentEpisode = _sut.GetCurrentEpisode(_mockSystemClock.Object);
        currentEpisode.EarningsProfile.Instalments.Count.Should().Be(2);
    }

    [Test]
    public void ThenAdditionalPaymentsBeforeLastDayOfLearningArePreserved()
    {
        // Arrange
        var lastDayOfLearning = new DateTime(2024, 6, 15);

        // Act
        _sut.RemovalEarningsFollowingWithdrawal(lastDayOfLearning, _mockSystemClock.Object);

        // Assert
        var currentEpisode = _sut.GetCurrentEpisode(_mockSystemClock.Object);
        currentEpisode.EarningsProfile.AdditionalPayments.Count.Should().Be(2);
    }

    [Test]
    public void ThenInstalmentsAfterLastDayOfLearningAreRemoved()
    {
        // Arrange
        var lastDayOfLearning = new DateTime(2024, 3, 15);

        // Act
        _sut.RemovalEarningsFollowingWithdrawal(lastDayOfLearning, _mockSystemClock.Object);

        // Assert
        var currentEpisode = _sut.GetCurrentEpisode(_mockSystemClock.Object);
        currentEpisode.EarningsProfile.Instalments.Should().OnlyContain(x =>
            x.AcademicYear < 2324 ||
            (x.AcademicYear == 2324 && x.DeliveryPeriod <= 7));
    }

    [Test]
    public void ThenAdditionalPaymentsAfterLastDayOfLearningAreRemoved()
    {
        // Arrange
        var lastDayOfLearning = new DateTime(2024, 3, 15);

        // Act
        _sut.RemovalEarningsFollowingWithdrawal(lastDayOfLearning, _mockSystemClock.Object);

        // Assert
        var currentEpisode = _sut.GetCurrentEpisode(_mockSystemClock.Object);
        currentEpisode.EarningsProfile.AdditionalPayments.Should().OnlyContain(x =>
            x.AcademicYear < 2324 ||
            (x.AcademicYear == 2324 && x.DeliveryPeriod <= 7));
    }


    [Test]
    public void ThenInstalmentsArePreservedForTheLastMonthIfTheLearnerWasInLearningOnTheCensusDate()
    {
        // Arrange
        var lastDayOfLearning = new DateTime(2024, 3, 31);

        // Act
        _sut.RemovalEarningsFollowingWithdrawal(lastDayOfLearning, _mockSystemClock.Object);

        // Assert
        var currentEpisode = _sut.GetCurrentEpisode(_mockSystemClock.Object);
        currentEpisode.EarningsProfile.Instalments.Should().Contain(x =>
            x.AcademicYear == 2324 && x.DeliveryPeriod == 8);
    }


    [Test]
    public void ThenAdditionalPaymentsArePreservedForTheLastMonthIfTheLearnerWasInLearningOnTheCensusDate()
    {
        // Arrange
        var lastDayOfLearning = new DateTime(2024, 3, 31);

        // Act
        _sut.RemovalEarningsFollowingWithdrawal(lastDayOfLearning, _mockSystemClock.Object);

        // Assert
        var currentEpisode = _sut.GetCurrentEpisode(_mockSystemClock.Object);
        currentEpisode.EarningsProfile.AdditionalPayments.Should().Contain(x =>
            x.AcademicYear == 2324 && x.DeliveryPeriod == 8);
    }

    [Test]
    public void ThenEarningsRecalculatedEventIsCreated()
    {
        // Arrange
        var lastDayOfLearning = new DateTime(2024, 6, 30);

        // Act
        _sut.RemovalEarningsFollowingWithdrawal(lastDayOfLearning, _mockSystemClock.Object);

        // Assert
        var events = _sut.FlushEvents();
        events.Should().ContainSingle(e => e is EarningsRecalculatedEvent);
    }

    [Test]
    public void ThenNewEarningsProfileIdIsGenerated()
    {
        // Arrange
        var lastDayOfLearning = new DateTime(2024, 6, 30);

        // Act
        _sut.RemovalEarningsFollowingWithdrawal(lastDayOfLearning, _mockSystemClock.Object);

        // Assert
        var currentEpisode = _sut.GetCurrentEpisode(_mockSystemClock.Object);
        currentEpisode.EarningsProfile.EarningsProfileId.Should().NotBeEmpty();
    }
}