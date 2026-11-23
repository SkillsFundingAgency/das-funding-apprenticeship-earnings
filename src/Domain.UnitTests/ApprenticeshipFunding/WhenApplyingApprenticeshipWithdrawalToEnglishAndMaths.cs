using AutoFixture;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Calculations;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Interfaces;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Models.Apprenticeship;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Models.EnglishAndMaths;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Services;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.UnitTests.TestHelpers;
using SFA.DAS.Funding.ApprenticeshipEarnings.TestHelpers;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.Domain.UnitTests.ApprenticeshipFunding;

/// <summary>
/// English and maths courses on an apprenticeship (approved or not) are treated as withdrawn when the apprenticeship is withdrawn,
/// unless they were completed (or withdrawn) on or before the apprenticeship withdrawal date
/// </summary>
[TestFixture]
public class WhenApplyingApprenticeshipWithdrawalToEnglishAndMaths
{
    private readonly Fixture _fixture = new();
    private Mock<ISystemClockService> _mockSystemClock = null!;

    private const string CourseName = "English Level 2";
    private static readonly DateTime CourseStartDate = new(2026, 10, 1);
    private static readonly DateTime CoursePlannedEndDate = new(2027, 5, 15);
    private static readonly DateTime ApprenticeshipWithdrawalDate = new(2027, 2, 10);

    [SetUp]
    public void SetUp()
    {
        _mockSystemClock = new Mock<ISystemClockService>();
        _mockSystemClock.Setup(x => x.UtcNow).Returns(new DateTime(2027, 3, 1));
    }

    [TestCase(null, null, null, null, null, Description = "Apprenticeship not withdrawn")]
    [TestCase("2027-02-10", null, null, "2027-02-10", null, Description = "Course still in learning")]
    [TestCase("2027-02-10", null, "2027-01-20", null, "2027-01-20", Description = "Course completed before apprenticeship withdrawal")]
    [TestCase("2027-02-10", null, "2027-02-10", null, "2027-02-10", Description = "Course completed on apprenticeship withdrawal date")]
    [TestCase("2027-02-10", null, "2027-05-15", "2027-02-10", null, Description = "Course completed after apprenticeship withdrawal")]
    [TestCase("2027-02-10", "2026-12-31", null, "2026-12-31", null, Description = "Course withdrawn before apprenticeship withdrawal")]
    [TestCase("2027-02-10", "2027-03-31", null, "2027-02-10", null, Description = "Course withdrawn after apprenticeship withdrawal")]
    public void ThenTheEffectiveEndDatesAreCorrect(string? apprenticeshipWithdrawal, string? courseWithdrawal, string? courseCompletion, string? expectedWithdrawal, string? expectedCompletion)
    {
        // Arrange
        var course = CreateCourse(ToDate(courseWithdrawal), ToDate(courseCompletion));

        // Act
        var result = course.GetEffectiveEndDates(ToDate(apprenticeshipWithdrawal));

        // Assert
        result.WithdrawalDate.Should().Be(ToDate(expectedWithdrawal));
        result.CompletionDate.Should().Be(ToDate(expectedCompletion));
    }

    [Test]
    public void ThenLearningSupportIsKeptOnlyUpToTheApprenticeshipWithdrawalDate()
    {
        // Act
        var course = CreateCourse(null, null, ApprenticeshipWithdrawalDate);

        // Assert - October to January census dates only
        course.AdditionalPayments.Should().HaveCount(4);
        course.AdditionalPayments.Should().OnlyContain(x => x.DueDate <= ApprenticeshipWithdrawalDate);
        course.Instalments.Should().HaveCount(4);
    }

    [Test]
    public void ThenNoLearningSupportIsEarnedWhenApprenticeshipWithdrawnBeforeTheQualifyingPeriod()
    {
        // Act - 42 day qualifying period, withdrawn after 31 days
        var course = CreateCourse(null, null, new DateTime(2026, 10, 31));

        // Assert
        course.Instalments.Should().BeEmpty();
        course.AdditionalPayments.Should().BeEmpty();
    }

    [Test]
    public void ThenLearningSupportIsUnchangedWhenCourseCompletedBeforeApprenticeshipWithdrawal()
    {
        // Act
        var completionDate = new DateTime(2027, 1, 20);
        var course = CreateCourse(null, completionDate, ApprenticeshipWithdrawalDate);

        // Assert
        course.AdditionalPayments.Should().HaveCount(CreateLearningSupport(completionDate).Count);
    }

    [Test]
    public void ThenRemovingTheApprenticeshipWithdrawalRestoresTheInstalments()
    {
        // Arrange
        var withdrawnCourse = CreateCourse(null, null, ApprenticeshipWithdrawalDate);

        // Act
        var restoredCourse = withdrawnCourse.WithApprenticeshipWithdrawal(null);

        // Assert
        restoredCourse.Instalments.Should().HaveCount(7);
        restoredCourse.WithdrawalDate.Should().BeNull();
        restoredCourse.CompletionDate.Should().BeNull();
    }

    [Test]
    public void ThenCoursesOnAnApprovedApprenticeshipAreRecalculatedWhenTheApprenticeshipIsWithdrawn()
    {
        // Arrange
        var (_, episode) = CreateApprenticeshipWithCourse(isApproved: true);

        // Act
        episode.UpdateWithdrawalDate(ApprenticeshipWithdrawalDate, _mockSystemClock.Object);
        episode.ApplyWithdrawalToEnglishAndMaths(_mockSystemClock.Object);

        // Assert
        var course = episode.EarningsProfile!.MathsAndEnglishCourses.Single();
        course.Instalments.Select(x => (int)x.DeliveryPeriod).Should().BeEquivalentTo(new[] { 3, 4, 5, 6 });
        course.AdditionalPayments.Should().HaveCount(4);
        course.WithdrawalDate.Should().BeNull("the ILR withdrawal date of the course is kept as provided");
        episode.EarningsProfile.EnglishAndMathsCourseChanges.Changed.Should().ContainSingle().Which.Should().Be(course.Key);
    }

    [Test]
    public void ThenCoursesAreRestoredWhenTheApprenticeshipWithdrawalIsRemoved()
    {
        // Arrange
        var (_, episode) = CreateApprenticeshipWithCourse(isApproved: true);
        episode.UpdateWithdrawalDate(ApprenticeshipWithdrawalDate, _mockSystemClock.Object);
        episode.ApplyWithdrawalToEnglishAndMaths(_mockSystemClock.Object);

        // Act
        episode.UpdateWithdrawalDate(null, _mockSystemClock.Object);
        episode.ApplyWithdrawalToEnglishAndMaths(_mockSystemClock.Object);

        // Assert
        episode.EarningsProfile!.MathsAndEnglishCourses.Single().Instalments.Should().HaveCount(7);
    }

    [Test]
    public void ThenCoursesOnAnUnapprovedApprenticeshipAreRecalculatedWhenTheApprenticeshipIsWithdrawn()
    {
        // Arrange
        var (_, episode) = CreateApprenticeshipWithCourse(isApproved: false);

        // Act
        episode.UpdateWithdrawalDate(ApprenticeshipWithdrawalDate, _mockSystemClock.Object);
        episode.ApplyWithdrawalToEnglishAndMaths(_mockSystemClock.Object);

        // Assert
        var course = episode.EarningsProfile!.MathsAndEnglishCourses.Single();
        course.Instalments.Select(x => (int)x.DeliveryPeriod).Should().BeEquivalentTo(new[] { 3, 4, 5, 6 });
        course.AdditionalPayments.Should().HaveCount(4);
        course.WithdrawalDate.Should().BeNull("the ILR withdrawal date of the course is kept as provided");
    }

    [Test]
    public void ThenRemovedCoursesAreNotReinstated()
    {
        // Arrange
        var (_, episode) = CreateApprenticeshipWithCourse(isApproved: true);
        episode.UpdateEnglishAndMaths([], _mockSystemClock.Object);

        // Act
        episode.UpdateWithdrawalDate(ApprenticeshipWithdrawalDate, _mockSystemClock.Object);
        episode.ApplyWithdrawalToEnglishAndMaths(_mockSystemClock.Object);

        // Assert
        var course = episode.EarningsProfile!.MathsAndEnglishCourses.Single();
        course.IsRemoved.Should().BeTrue();
        course.Instalments.Should().BeEmpty();
        episode.EarningsProfile.EnglishAndMathsCourseChanges.HasChanges.Should().BeFalse();
    }

    private (ApprenticeshipLearning, ApprenticeshipEpisode) CreateApprenticeshipWithCourse(bool isApproved)
    {
        var learning = _fixture.CreateLearningWithApprenticeship(new DateTime(2026, 10, 1), new DateTime(2027, 7, 31), 5000m, age: 25);
        var episode = learning.Episodes.First();
        episode.CalculateOnProgramme(learning, _mockSystemClock.Object, string.Empty, isApproved);
        episode.UpdateEnglishAndMaths([CreateCourse(null, null)], _mockSystemClock.Object);
        return (learning, episode);
    }

    private static EnglishAndMaths CreateCourse(DateTime? withdrawalDate, DateTime? completionDate, DateTime? apprenticeshipWithdrawalDate = null)
    {
        var periodInLearning = PeriodInLearningHelper.Create(CourseStartDate, completionDate ?? withdrawalDate ?? CoursePlannedEndDate, CoursePlannedEndDate);

        return new EnglishAndMaths(CourseStartDate, CoursePlannedEndDate, CourseName, CourseName, 700m, withdrawalDate, completionDate, null, null,
            new List<IPeriodInLearning> { periodInLearning }, CreateLearningSupport(withdrawalDate ?? completionDate ?? CoursePlannedEndDate), apprenticeshipWithdrawalDate);
    }

    private static List<EnglishAndMathsAdditionalPayment> CreateLearningSupport(DateTime endDate)
    {
        return LearningSupportPayments
            .GenerateLearningSupportPayments(CourseStartDate, endDate)
            .Select(p => new EnglishAndMathsAdditionalPayment(p.AcademicYear, p.DeliveryPeriod, p.Amount, p.DueDate, p.AdditionalPaymentType))
            .ToList();
    }

    private static DateTime? ToDate(string? value) => value == null ? null : DateTime.Parse(value);
}
