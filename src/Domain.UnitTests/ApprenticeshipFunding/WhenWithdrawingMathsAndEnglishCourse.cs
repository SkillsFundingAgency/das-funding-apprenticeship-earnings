using System;
using System.Collections.Generic;
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
public class WhenWithdrawingMathsAndEnglishCourse
{
    private readonly Fixture _fixture = new();
    private Apprenticeship.Apprenticeship _sut;
    private Mock<ISystemClockService> _mockSystemClock;
    private string _courseName = "English Level 2";

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

        episode.CalculateOnProgram(_sut, _mockSystemClock.Object);

        var mathsAndEnglishCourse = new MathsAndEnglish(new DateTime(2024, 1, 1), new DateTime(2024, 12, 31), _courseName, 1200m, new List<MathsAndEnglishInstalment>
        {
            new MathsAndEnglishInstalment(2324, 6, 100m, MathsAndEnglishInstalmentType.Regular, false),
            new MathsAndEnglishInstalment(2324, 7, 100m, MathsAndEnglishInstalmentType.Regular, false),
            new MathsAndEnglishInstalment(2324, 8, 100m, MathsAndEnglishInstalmentType.Regular, false),
            new MathsAndEnglishInstalment(2324, 9, 100m, MathsAndEnglishInstalmentType.Regular, false),
            new MathsAndEnglishInstalment(2324, 10, 100m, MathsAndEnglishInstalmentType.Regular, false),
            new MathsAndEnglishInstalment(2324, 11, 100m, MathsAndEnglishInstalmentType.Regular, false),
            new MathsAndEnglishInstalment(2324, 12, 100m, MathsAndEnglishInstalmentType.Regular, false),
            new MathsAndEnglishInstalment(2425, 1, 100m, MathsAndEnglishInstalmentType.Regular, false),
            new MathsAndEnglishInstalment(2425, 2, 100m, MathsAndEnglishInstalmentType.Regular, false),
            new MathsAndEnglishInstalment(2425, 3, 100m, MathsAndEnglishInstalmentType.Regular, false),
            new MathsAndEnglishInstalment(2425, 4, 100m, MathsAndEnglishInstalmentType.Regular, false),
            new MathsAndEnglishInstalment(2425, 5, 100m, MathsAndEnglishInstalmentType.Regular, false)
        }, null, null, null, null);

        episode.UpdateEnglishAndMaths(new() { mathsAndEnglishCourse }, _mockSystemClock.Object);
    }

    [Test]
    public void Then_Instalments_Before_WithdrawalDate_Are_Preserved()
    {
        // Arrange
        var withdrawalDate = new DateTime(2024, 4, 15);

        // Act
        _sut.WithdrawMathsAndEnglishCourse(_courseName, withdrawalDate, _mockSystemClock.Object);

        // Assert
        var episode = _sut.ApprenticeshipEpisodes.First();
        var course = episode.EarningsProfile.MathsAndEnglishCourses.Single(x => x.Course == _courseName);

        course.Instalments.Where(x => !x.IsAfterLearningEnded).Should().OnlyContain(x =>
            x.AcademicYear < 2324 ||
            (x.AcademicYear == 2324 && x.DeliveryPeriod <= 8)); // up to April
    }

    [Test]
    public void Then_Instalments_After_WithdrawalDate_Are_SoftDeleted()
    {
        // Arrange
        var withdrawalDate = new DateTime(2024, 4, 15);

        // Act
        _sut.WithdrawMathsAndEnglishCourse(_courseName, withdrawalDate, _mockSystemClock.Object);

        // Assert
        var episode = _sut.ApprenticeshipEpisodes.First();
        var course = episode.EarningsProfile.MathsAndEnglishCourses.Single(x => x.Course == _courseName);

        course.Instalments.Where(x => !x.IsAfterLearningEnded).Should().NotContain(x =>
            x.AcademicYear == 2324 && x.DeliveryPeriod > 8);
        course.Instalments.Count.Should().Be(12); //total count including soft deleted records should still be 12
    }

    [Test]
    public void Then_A_Single_Instalment_Where_The_Withdrawal_Date_Is_Before_The_Census_Date_Is_Kept()
    {
        // Arrange
        var withdrawalDate = new DateTime(2024, 1, 15);

        // Act
        _sut.WithdrawMathsAndEnglishCourse(_courseName, withdrawalDate, _mockSystemClock.Object);

        // Assert
        var episode = _sut.ApprenticeshipEpisodes.First();
        var course = episode.EarningsProfile.MathsAndEnglishCourses.Single(x => x.Course == _courseName);

        course.Instalments.Where(x => !x.IsAfterLearningEnded).Should().OnlyContain(x =>
            x.AcademicYear == 2324 && x.DeliveryPeriod == 6);
        course.Instalments.Count(x => !x.IsAfterLearningEnded).Should().Be(1);
        course.Instalments.Count.Should().Be(12); //total count including soft deleted records should still be 12
    }

    [Test]
    public void Then_No_Instalments_Are_Kept_When_Withdrawing_Back_To_The_Start()
    {
        // Arrange
        var withdrawalDate = new DateTime(2024, 1, 1);

        // Act
        _sut.WithdrawMathsAndEnglishCourse(_courseName, withdrawalDate, _mockSystemClock.Object);

        // Assert
        var episode = _sut.ApprenticeshipEpisodes.First();
        var course = episode.EarningsProfile.MathsAndEnglishCourses.Single(x => x.Course == _courseName);

        course.Instalments.Count(x => !x.IsAfterLearningEnded).Should().Be(0);
        course.Instalments.Count.Should().Be(12); //total count including soft deleted records should still be 12
    }

    [Test]
    public void Then_WithdrawalDate_Is_Stored_On_Course()
    {
        // Arrange
        var withdrawalDate = new DateTime(2024, 6, 30);

        // Act
        _sut.WithdrawMathsAndEnglishCourse(_courseName, withdrawalDate, _mockSystemClock.Object);

        // Assert
        var episode = _sut.ApprenticeshipEpisodes.First();
        var course = episode.EarningsProfile.MathsAndEnglishCourses.Single(x => x.Course == _courseName);

        course.WithdrawalDate.Should().Be(withdrawalDate);
    }

    [Test]
    public void Then_New_EarningsProfile_Version_Is_Generated()
    {
        // Arrange
        var initialVersion = _sut.ApprenticeshipEpisodes.First().EarningsProfile.Version;
        var withdrawalDate = new DateTime(2024, 7, 1);

        // Act
        _sut.WithdrawMathsAndEnglishCourse(_courseName, withdrawalDate, _mockSystemClock.Object);

        // Assert
        var updatedVersion = _sut.ApprenticeshipEpisodes.First().EarningsProfile.Version;
        updatedVersion.Should().NotBe(initialVersion);
    }

    [Test]
    public void Then_Throws_If_Course_Does_Not_Exist()
    {
        // Arrange
        var withdrawalDate = new DateTime(2024, 5, 31);

        // Act
        var action = () => _sut.WithdrawMathsAndEnglishCourse("Nonexistent Course", withdrawalDate, _mockSystemClock.Object);

        // Assert
        action.Should().Throw<ArgumentException>()
            .WithMessage("*No english and maths course found for course name Nonexistent Course*");
    }
}