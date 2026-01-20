using AutoFixture;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Apprenticeship;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Interfaces;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Services;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.UnitTests.TestHelpers;
using SFA.DAS.Funding.ApprenticeshipEarnings.TestHelpers;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.Domain.UnitTests.ApprenticeshipFunding;

[TestFixture]
public class WhenWithdrawingEnglishAndMathsCourse
{
    private readonly Fixture _fixture = new();
    private Apprenticeship.Apprenticeship _apprenticeship;
    private Apprenticeship.ApprenticeshipEpisode _sut;
    private Mock<ISystemClockService> _mockSystemClock;
    private string _courseName = "English Level 2";
    private List<MathsAndEnglish> _courses;

    [SetUp]
    public void SetUp()
    {
        _mockSystemClock = new Mock<ISystemClockService>();
        _mockSystemClock.Setup(x => x.UtcNow).Returns(new DateTime(2024, 12, 24));

        var actualStartDate = new DateTime(2024, 1, 1);
        var plannedEndDate = new DateTime(2024, 12, 31);
        var agreedPrice = 12000m;

        _apprenticeship = _fixture.CreateApprenticeship(actualStartDate, plannedEndDate, agreedPrice);
        _sut = _apprenticeship.ApprenticeshipEpisodes.First();

        _sut.CalculateOnProgram(_apprenticeship, _mockSystemClock.Object);

        _sut.UpdateEnglishAndMaths(CreateTestEnglishAndMathsCourses(), _mockSystemClock.Object);
    }

    [Test]
    public void Then_Instalments_Before_WithdrawalDate_Are_Preserved()
    {
        // Arrange
        var withdrawalDate = new DateTime(2024, 4, 15);

        // Act
        _sut.UpdateEnglishAndMaths(CreateTestEnglishAndMathsCourses(withdrawalDate), _mockSystemClock.Object);

        // Assert
        var course = _sut.EarningsProfile.MathsAndEnglishCourses.Single(x => x.Course == _courseName);

        course.Instalments.Should().OnlyContain(x =>
            x.AcademicYear < 2324 ||
            (x.AcademicYear == 2324 && x.DeliveryPeriod <= 8)); // up to April
    }

    [Test]
    public void Then_Instalments_After_WithdrawalDate_Are_Deleted()
    {
        // Arrange
        var withdrawalDate = new DateTime(2024, 4, 15);

        // Act
        _sut.UpdateEnglishAndMaths(CreateTestEnglishAndMathsCourses(withdrawalDate), _mockSystemClock.Object);

        // Assert
        var course = _sut.EarningsProfile.MathsAndEnglishCourses.Single(x => x.Course == _courseName);

        course.Instalments.Should().NotContain(x =>
            x.AcademicYear == 2324 && x.DeliveryPeriod > 8);
    }

    [Test]
    public void Then_A_Single_Instalment_Where_The_Withdrawal_Date_Is_Before_The_Census_Date_Is_Kept()
    {
        // Arrange
        var withdrawalDate = new DateTime(2024, 1, 15);

        // Act
        _sut.UpdateEnglishAndMaths(CreateTestEnglishAndMathsCourses(withdrawalDate), _mockSystemClock.Object);

        // Assert
        var course = _sut.EarningsProfile.MathsAndEnglishCourses.Single(x => x.Course == _courseName);

        course.Instalments.Should().OnlyContain(x =>
            x.AcademicYear == 2324 && x.DeliveryPeriod == 6);
        course.Instalments.Count.Should().Be(1);
    }

    [Test]
    public void Then_No_Instalments_Are_Kept_When_Withdrawing_Back_To_The_Start()
    {
        // Arrange
        var withdrawalDate = new DateTime(2024, 1, 1);

        // Act
        _sut.UpdateEnglishAndMaths(CreateTestEnglishAndMathsCourses(withdrawalDate), _mockSystemClock.Object);

        // Assert
        var course = _sut.EarningsProfile.MathsAndEnglishCourses.Single(x => x.Course == _courseName);

        course.Instalments.Count.Should().Be(0);
    }

    [Test]
    public void Then_WithdrawalDate_Is_Stored_On_Course()
    {
        // Arrange
        var withdrawalDate = new DateTime(2024, 6, 30);

        // Act
        _sut.UpdateEnglishAndMaths(CreateTestEnglishAndMathsCourses(withdrawalDate), _mockSystemClock.Object);

        // Assert
        var course = _sut.EarningsProfile.MathsAndEnglishCourses.Single(x => x.Course == _courseName);

        course.WithdrawalDate.Should().Be(withdrawalDate);
    }

    [Test]
    public void Then_New_EarningsProfile_Version_Is_Generated()
    {
        // Arrange
        var initialVersion = _sut.EarningsProfile.Version;
        var withdrawalDate = new DateTime(2024, 7, 1);

        // Act
        _sut.UpdateEnglishAndMaths(CreateTestEnglishAndMathsCourses(withdrawalDate), _mockSystemClock.Object);

        // Assert
        var updatedVersion = _sut.EarningsProfile.Version;
        updatedVersion.Should().NotBe(initialVersion);
    }

    private List<MathsAndEnglish> CreateTestEnglishAndMathsCourses(DateTime? withdrawalDate = null)
    {
        var startDate = new DateTime(2024, 1, 1);
        var endDate = new DateTime(2024, 12, 31);

        var periodInLearning = PeriodInLearningHelper.Create(startDate, endDate, endDate);

        var mathsAndEnglishCourse = new MathsAndEnglish(startDate, endDate, _courseName, _courseName, 1200m, withdrawalDate, null, null, null, new List<IPeriodInLearning> { periodInLearning });
        return new List<MathsAndEnglish> { mathsAndEnglishCourse };
    }
}