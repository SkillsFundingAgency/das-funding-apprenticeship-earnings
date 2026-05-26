using AutoFixture;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Extensions;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Models;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Models.Apprenticeship;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Services;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.UnitTests.TestHelpers;
using System;
using System.Linq;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.Domain.UnitTests.ApprenticeshipFunding;

[TestFixture]
public class WhenRemovingEarnings_WithdrawalWithMultiplePeriodsInLearningAndQualification
{
    private Fixture _fixture = new();
    private ApprenticeshipLearning _apprenticeship;
    private Mock<ISystemClockService> _mockSystemClock;

    [TestCase(168, 42, true)]
    [TestCase(168, 41, false)]
    [TestCase(167, 14, true)]
    [TestCase(167, 13, false)]
    [TestCase(14, 14, true)]
    [TestCase(14, 13, false)]
    [TestCase(13, 1, true)]
    [TestCase(1, 1, true)]
    public void ThenInstalmentsForPreviousPeriodInLearningArePreservedButInstalmentsForCurrentPeriodAreRemovedBasedOnQualifyingPeriod(
        int returnPlannedDuration, int returnWithdrawalAfterDays, bool expectedToQualify)
    {
        // Arrange
        // Original Period: Meets the 42 day qualifying period before and after considering the pause.
        var originalStartDate = new DateTime(2023, 1, 1);
        var originalPlannedEndDate = new DateTime(2023, 12, 31);
        var pauseDate = new DateTime(2023, 2, 20); // 51 days in learning, > 42 days qualifying period

        SetupSystemClock(new DateTime(2023, 6, 24));
        SetupApprenticeship(originalStartDate, originalPlannedEndDate, 12000m);
        var currentEpisode = _apprenticeship.GetCurrentEpisode(_mockSystemClock.Object);

        // Pause first period in learning
        currentEpisode.UpdatePause(pauseDate);
        currentEpisode.UpdatePeriodsInLearning(new System.Collections.Generic.List<ApprenticeshipPeriodInLearning>
        {
            new ApprenticeshipPeriodInLearning(currentEpisode.EpisodeKey, originalStartDate, pauseDate, originalPlannedEndDate)
        });
        _apprenticeship.Calculate(_mockSystemClock.Object, string.Empty);
        
        // Expect instalments to be retained
        currentEpisode.EarningsProfile.Instalments.Should().NotBeEmpty();
        var firstPILExpectedInstalmentCount = currentEpisode.EarningsProfile.Instalments.Count;

        // Create second period in learning (return/restart)
        var returnStartDate = new DateTime(2023, 4, 30);
        var returnPlannedEndDate = returnStartDate.AddDays(returnPlannedDuration - 1);
        var withdrawalDate = returnStartDate.AddDays(returnWithdrawalAfterDays - 1);

        currentEpisode.UpdatePause(null);
        currentEpisode.UpdatePeriodsInLearning(new System.Collections.Generic.List<ApprenticeshipPeriodInLearning>
        {
            new ApprenticeshipPeriodInLearning(currentEpisode.EpisodeKey, originalStartDate, pauseDate, originalPlannedEndDate),
            new ApprenticeshipPeriodInLearning(currentEpisode.EpisodeKey, returnStartDate, withdrawalDate, returnPlannedEndDate)
        });

        // Trigger the withdrawal and re-calc
        currentEpisode.UpdateWithdrawalDate(withdrawalDate, _mockSystemClock.Object);
        _apprenticeship.Calculate(_mockSystemClock.Object, string.Empty);

        // Assert

        // Instalments from the first period should be retained
        var totalInstalmentCount = currentEpisode.EarningsProfile.Instalments.Count;
        totalInstalmentCount.Should().BeGreaterThanOrEqualTo(firstPILExpectedInstalmentCount);

        if (expectedToQualify)
        {
            //expect instalments from the second period
            totalInstalmentCount.Should().BeGreaterThan(firstPILExpectedInstalmentCount);
        }
        else
        {
            //expect no instalments from the second period
            totalInstalmentCount.Should().Be(firstPILExpectedInstalmentCount);
        }
    }

    private void SetupSystemClock(DateTime currentDateTime)
    {
        _mockSystemClock = new Mock<ISystemClockService>();
        _mockSystemClock.Setup(x => x.UtcNow).Returns(currentDateTime);
    }

    private void SetupApprenticeship(DateTime actualStartDate, DateTime plannedEndDate, decimal agreedPrice)
    {
        _apprenticeship = _fixture.CreateLearningWithApprenticeship(actualStartDate, plannedEndDate, agreedPrice);
        var episode = _apprenticeship.Episodes.First();

        episode.CalculateOnProgram(_apprenticeship, _mockSystemClock.Object, string.Empty);
    }
}
