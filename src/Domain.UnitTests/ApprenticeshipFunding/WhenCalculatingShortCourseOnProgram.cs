using AutoFixture;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Models;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Models.Apprenticeship;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Models.ShortCourse;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Services;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.UnitTests.TestHelpers;
using SFA.DAS.Funding.ApprenticeshipEarnings.Types;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.Domain.UnitTests.ApprenticeshipFunding;

[TestFixture]
internal class WhenCalculatingShortCourseOnProgram
{
    private readonly Fixture _fixture = new();
    private ShortCourseLearning _learning;
    private ShortCourseEpisode _episode;
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

        _learning = _fixture.CreateLearningWithShortCourse(startDate, endDate, _agreedPrice);
        _episode = _learning.Episodes.Single();
    }

    [Test]
    public void ThenEarningsProfileIsCreatedIfNoneExists()
    {
        // Act
        _episode.CalculateShortCourseOnProgram(calculationData: "test-data");

        // Assert
        _episode.EarningsProfile.Should().NotBeNull();
        _episode.EarningsProfile.EarningsProfileId.Should().NotBeEmpty();
    }

    [Test]
    public void ThenInstalmentsAreGenerated()
    {
        // Act
        _episode.CalculateShortCourseOnProgram(calculationData: "test-data");

        // Assert
        _episode.EarningsProfile.Instalments.Should().NotBeEmpty();
    }

    [Test]
    public void ThenCompletionPaymentIsCalculated()
    {
        // Act
        _episode.CalculateShortCourseOnProgram(calculationData: "test-data");

        // Assert
        _episode.EarningsProfile.CompletionPayment.Should().Be(_agreedPrice*0.7m);
    }

    [Test]
    public void ThenOnProgramTotalIsCalculated()
    {
        // Act
        _episode.CalculateShortCourseOnProgram(calculationData: "test-data");

        // Assert
        _episode.EarningsProfile.OnProgramTotal.Should().Be(_agreedPrice * 0.3m);
    }

    [Test]
    public void ThenEarningsProfileIsUpdatedIfItAlreadyExists()
    {
        // Arrange
        _episode.CalculateShortCourseOnProgram(calculationData: "initial");

        var originalProfileId = _episode.EarningsProfile.EarningsProfileId;

        // Act
        _episode.CalculateShortCourseOnProgram(calculationData: "updated");

        // Assert
        _episode.EarningsProfile.EarningsProfileId.Should().Be(originalProfileId);
    }

    [Test]
    public void ThenCalculationDataIsUpdatedWhenRecalculated()
    {
        // Arrange
        _episode.CalculateShortCourseOnProgram(calculationData: "initial");

        // Act
        _episode.CalculateShortCourseOnProgram(calculationData: "updated");

        // Assert
        _episode.EarningsProfile.CalculationData.Should().Be("updated");
    }

    [Test]
    public void WhenApproved_ThenBothMilestonesAreMarkedPayableIfRecorded()
    {
        // Arrange - milestones set, earnings calculated while unapproved
        _episode.UpdateMilestones(new List<Milestone> { Milestone.ThirtyPercentLearningComplete, Milestone.LearningComplete });
        _episode.CalculateShortCourseOnProgram(calculationData: "test-data");
        _episode.EarningsProfile.Instalments.Should().AllSatisfy(i => i.IsPayable.Should().BeFalse());

        // Act
        _episode.Approve();

        // Assert
        _episode.EarningsProfile.Instalments.Should().AllSatisfy(i => i.IsPayable.Should().BeTrue());
    }

    [Test]
    public void WhenPreviouslyRemoved_ThenIsRemovedIsClearedAndEarningsAreRestored()
    {
        // Arrange
        _episode.CalculateShortCourseOnProgram(calculationData: "initial");
        _episode.Delete();
        _episode.EarningsProfile.Instalments.Should().BeEmpty();

        // Act
        _episode.CalculateShortCourseOnProgram(calculationData: "reinstated");

        // Assert
        _episode.IsRemoved.Should().BeFalse();
        _episode.EarningsProfile.Instalments.Should().NotBeEmpty();
        _episode.EarningsProfile.OnProgramTotal.Should().Be(_agreedPrice * 0.3m);
        _episode.EarningsProfile.CompletionPayment.Should().Be(_agreedPrice * 0.7m);
    }

    [Test]
    public void WhenApproved_MilestonesIsMarkedPayableIfRecorded()
    {
        // Arrange - only the 30% milestone achieved; clear withdrawal date so both instalments are generated
        _episode.UpdateWithdrawalDate(null);
        _episode.UpdateMilestones(new List<Milestone> { Milestone.ThirtyPercentLearningComplete });
        _episode.CalculateShortCourseOnProgram(calculationData: "test-data");

        // Act
        _episode.Approve();

        // Assert
        _episode.EarningsProfile.Instalments.Single(i => i.Type == ShortCourseInstalmentType.ThirtyPercentLearningComplete).IsPayable.Should().BeTrue();
        _episode.EarningsProfile.Instalments.Single(i => i.Type == ShortCourseInstalmentType.LearningComplete).IsPayable.Should().BeFalse();
    }
}