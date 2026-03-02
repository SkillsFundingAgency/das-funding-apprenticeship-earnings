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
internal class WhenCalculatingShortCourseOnProgram
{
    private readonly Fixture _fixture = new();
    private Apprenticeship.Apprenticeship _apprenticeship;
    private ApprenticeshipEpisode _episode;
    private Mock<ISystemClockService> _mockSystemClock;
    private decimal _agreedPrice;

    [SetUp]
    public void SetUp()
    {
        _mockSystemClock = new Mock<ISystemClockService>();
        _mockSystemClock.Setup(x => x.UtcNow).Returns(new DateTime(2024, 12, 24));

        var startDate = new DateTime(2024, 1, 1);
        var endDate = new DateTime(2024, 3, 31);
        
        _agreedPrice = 3000m;

        _apprenticeship = _fixture.CreateApprenticeship(startDate, endDate, _agreedPrice);
        _episode = _apprenticeship.ApprenticeshipEpisodes.Single();
    }

    [Test]
    public void ThenEarningsProfileIsCreatedIfNoneExists()
    {
        // Act
        _episode.CalculateShortCourseOnProgram(
            _apprenticeship,
            _mockSystemClock.Object,
            isApproved: true,
            calculationData: "test-data");

        // Assert
        _episode.EarningsProfile.Should().NotBeNull();
        _episode.EarningsProfile.EarningsProfileId.Should().NotBeEmpty();
    }

    [Test]
    public void ThenInstalmentsAreGenerated()
    {
        // Act
        _episode.CalculateShortCourseOnProgram(
            _apprenticeship,
            _mockSystemClock.Object,
            isApproved: true,
            calculationData: "test-data");

        // Assert
        _episode.EarningsProfile.Instalments.Should().NotBeEmpty();
    }

    [Test]
    public void ThenCompletionPaymentIsCalculated()
    {
        // Act
        _episode.CalculateShortCourseOnProgram(
            _apprenticeship,
            _mockSystemClock.Object,
            isApproved: true,
            calculationData: "test-data");

        // Assert
        _episode.EarningsProfile.CompletionPayment.Should().Be(_agreedPrice*0.7m);
    }

    [Test]
    public void ThenOnProgramTotalIsCalculated()
    {
        // Act
        _episode.CalculateShortCourseOnProgram(
            _apprenticeship,
            _mockSystemClock.Object,
            isApproved: true,
            calculationData: "test-data");

        // Assert
        _episode.EarningsProfile.OnProgramTotal.Should().Be(_agreedPrice * 0.3m);
    }

    [Test]
    public void ThenEarningsProfileIsUpdatedIfItAlreadyExists()
    {
        // Arrange
        _episode.CalculateShortCourseOnProgram(
            _apprenticeship,
            _mockSystemClock.Object,
            isApproved: false,
            calculationData: "initial");

        var originalProfileId = _episode.EarningsProfile.EarningsProfileId;

        // Act
        _episode.CalculateShortCourseOnProgram(
            _apprenticeship,
            _mockSystemClock.Object,
            isApproved: true,
            calculationData: "updated");

        // Assert
        _episode.EarningsProfile.EarningsProfileId.Should().Be(originalProfileId);
    }

    [Test]
    public void ThenCalculationDataIsUpdatedWhenRecalculated()
    {
        // Arrange
        _episode.CalculateShortCourseOnProgram(
            _apprenticeship,
            _mockSystemClock.Object,
            isApproved: false,
            calculationData: "initial");

        // Act
        _episode.CalculateShortCourseOnProgram(
            _apprenticeship,
            _mockSystemClock.Object,
            isApproved: true,
            calculationData: "updated");

        // Assert
        _episode.EarningsProfile.CalculationData.Should().Be("updated");
    }
}