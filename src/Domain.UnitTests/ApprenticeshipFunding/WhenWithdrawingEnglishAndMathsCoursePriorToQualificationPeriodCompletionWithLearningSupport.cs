using AutoFixture;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Calculations;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Interfaces;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Models;
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
/// Learning support for an English and maths course must not be earned if the learner is withdrawn from the course before the end of the qualifying period
/// </summary>
[TestFixture]
public class WhenWithdrawingEnglishAndMathsCoursePriorToQualificationPeriodCompletionWithLearningSupport
{
    private readonly Fixture _fixture = new();
    private const string CourseName = "English Level 2";

    [TestCase(168, 42, true)]
    [TestCase(168, 41, false)]
    [TestCase(167, 14, true)]
    [TestCase(167, 13, false)]
    [TestCase(14, 14, true)]
    [TestCase(14, 13, false)]
    public void ThenLearningSupportIsKeptOnlyIfLearnerQualified(int plannedDuration, int daysInLearning, bool expectedToQualify)
    {
        // Arrange
        var startDate = new DateTime(2024, 1, 20);
        var endDate = startDate.AddDays(plannedDuration - 1);
        var withdrawalDate = startDate.AddDays(daysInLearning - 1);

        // Act
        var course = CreateCourse(startDate, endDate, withdrawalDate);

        // Assert
        if (expectedToQualify)
            course.AdditionalPayments.Should().NotBeEmpty();
        else
            course.AdditionalPayments.Should().BeEmpty();
    }

    [Test]
    public void ThenLearningSupportIsRemovedWhenCourseIsUpdatedWithNonQualifyingWithdrawal()
    {
        // Arrange
        var mockSystemClock = new Mock<ISystemClockService>();
        mockSystemClock.Setup(x => x.UtcNow).Returns(new DateTime(2024, 6, 24));

        var startDate = new DateTime(2024, 1, 1);
        var endDate = new DateTime(2024, 12, 31);
        var learning = _fixture.CreateLearningWithApprenticeship(startDate, endDate, 12000m, age: 25);
        var episode = learning.Episodes.First();
        episode.CalculateOnProgram(learning, mockSystemClock.Object, string.Empty);
        episode.UpdateEnglishAndMaths([CreateCourse(startDate, endDate, null)], mockSystemClock.Object);

        // Act - withdrawn after 31 days of a 366 day course
        episode.UpdateEnglishAndMaths([CreateCourse(startDate, endDate, new DateTime(2024, 1, 31))], mockSystemClock.Object);

        // Assert
        var course = episode.EarningsProfile.MathsAndEnglishCourses.Single(x => x.Course == CourseName);
        course.Instalments.Should().BeEmpty();
        course.AdditionalPayments.Should().BeEmpty();
    }

    [Test]
    public void ThenLearningSupportBeforeWithdrawalIsKeptWhenLearnerQualified()
    {
        // Arrange
        var startDate = new DateTime(2024, 1, 1);
        var endDate = new DateTime(2024, 12, 31);
        var withdrawalDate = new DateTime(2024, 3, 31);

        // Act
        var course = CreateCourse(startDate, endDate, withdrawalDate);

        // Assert
        course.AdditionalPayments.Should().HaveCount(3);
    }

    private static EnglishAndMaths CreateCourse(DateTime startDate, DateTime endDate, DateTime? withdrawalDate)
    {
        var learningSupport = LearningSupportPayments
            .GenerateLearningSupportPayments(startDate, withdrawalDate ?? endDate)
            .Select(p => new EnglishAndMathsAdditionalPayment(p.AcademicYear, p.DeliveryPeriod, p.Amount, p.DueDate, p.AdditionalPaymentType))
            .ToList();

        var periodInLearning = PeriodInLearningHelper.Create(startDate, endDate, endDate);

        return new EnglishAndMaths(startDate, endDate, CourseName, CourseName, 1200m, withdrawalDate, null, null, null, new List<IPeriodInLearning> { periodInLearning }, learningSupport);
    }
}
