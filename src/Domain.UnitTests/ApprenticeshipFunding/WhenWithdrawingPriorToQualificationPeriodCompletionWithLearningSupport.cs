using AutoFixture;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Calculations;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Models;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Models.Apprenticeship;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Services;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.UnitTests.TestHelpers;
using System;
using System.Linq;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.Domain.UnitTests.ApprenticeshipFunding;

/// <summary>
/// Learning support for the on programme aim must not be earned if the learner is withdrawn before the end of the qualifying period
/// </summary>
[TestFixture]
public class WhenWithdrawingPriorToQualificationPeriodCompletionWithLearningSupport
{
    private readonly Fixture _fixture = new();
    private ApprenticeshipLearning _apprenticeship;
    private ApprenticeshipEpisode _episode;
    private Mock<ISystemClockService> _mockSystemClock;

    [SetUp]
    public void SetUp()
    {
        _mockSystemClock = new Mock<ISystemClockService>();
        _mockSystemClock.Setup(x => x.UtcNow).Returns(new DateTime(2024, 6, 24));
    }

    [TestCase(168, 42, true)]
    [TestCase(168, 41, false)]
    [TestCase(167, 14, true)]
    [TestCase(167, 13, false)]
    [TestCase(14, 14, true)]
    [TestCase(14, 13, false)]
    public void ThenLearningSupportAddedAfterWithdrawalIsKeptOnlyIfLearnerQualified(int plannedDuration, int daysInLearning, bool expectedToQualify)
    {
        // Arrange
        var startDate = new DateTime(2024, 1, 20);
        var withdrawalDate = startDate.AddDays(daysInLearning - 1);
        SetupApprenticeship(startDate, startDate.AddDays(plannedDuration - 1));

        // Act - same order as UpdateOnProgrammeCommandHandler: withdrawal, calculate, then add learning support
        _episode.UpdateWithdrawalDate(withdrawalDate, _mockSystemClock.Object);
        _apprenticeship.Calculate(_mockSystemClock.Object, string.Empty);
        _episode.AddAdditionalEarnings(
            LearningSupportPayments.GenerateLearningSupportPayments(startDate, withdrawalDate),
            InstalmentTypes.LearningSupport,
            _mockSystemClock.Object);

        // Assert
        AssertLearningSupport(expectedToQualify);
    }

    [TestCase(168, 42, true)]
    [TestCase(168, 41, false)]
    [TestCase(14, 14, true)]
    [TestCase(14, 13, false)]
    public void ThenExistingLearningSupportIsKeptOnRecalculationOnlyIfLearnerQualified(int plannedDuration, int daysInLearning, bool expectedToQualify)
    {
        // Arrange - learning support already on the profile, learner withdrawn afterwards
        var startDate = new DateTime(2024, 1, 20);
        var withdrawalDate = startDate.AddDays(daysInLearning - 1);
        var plannedEndDate = startDate.AddDays(plannedDuration - 1);
        SetupApprenticeship(startDate, plannedEndDate);

        _episode.AddAdditionalEarnings(
            LearningSupportPayments.GenerateLearningSupportPayments(startDate, plannedEndDate),
            InstalmentTypes.LearningSupport,
            _mockSystemClock.Object);

        // Act
        _episode.UpdateWithdrawalDate(withdrawalDate, _mockSystemClock.Object);
        _apprenticeship.Calculate(_mockSystemClock.Object, string.Empty);

        // Assert
        AssertLearningSupport(expectedToQualify);
    }

    [Test]
    public void ThenLearningSupportBeforeWithdrawalIsKeptWhenLearnerQualified()
    {
        // Arrange
        var startDate = new DateTime(2024, 1, 1);
        var withdrawalDate = new DateTime(2024, 3, 31);
        SetupApprenticeship(startDate, new DateTime(2024, 12, 31));

        // Act
        _episode.UpdateWithdrawalDate(withdrawalDate, _mockSystemClock.Object);
        _apprenticeship.Calculate(_mockSystemClock.Object, string.Empty);
        _episode.AddAdditionalEarnings(
            LearningSupportPayments.GenerateLearningSupportPayments(startDate, withdrawalDate),
            InstalmentTypes.LearningSupport,
            _mockSystemClock.Object);

        // Assert
        _episode.EarningsProfile.AdditionalPayments
            .Where(x => x.AdditionalPaymentType == InstalmentTypes.LearningSupport)
            .Should().HaveCount(3);
    }

    private void AssertLearningSupport(bool expectedToQualify)
    {
        var learningSupport = _episode.EarningsProfile.AdditionalPayments
            .Where(x => x.AdditionalPaymentType == InstalmentTypes.LearningSupport)
            .ToList();

        if (expectedToQualify)
            learningSupport.Should().NotBeEmpty();
        else
            learningSupport.Should().BeEmpty();
    }

    private void SetupApprenticeship(DateTime startDate, DateTime plannedEndDate)
    {
        _apprenticeship = _fixture.CreateLearningWithApprenticeship(startDate, plannedEndDate, 12000m, age: 25);
        _episode = _apprenticeship.Episodes.First();
        _episode.CalculateOnProgram(_apprenticeship, _mockSystemClock.Object, string.Empty);
    }
}
