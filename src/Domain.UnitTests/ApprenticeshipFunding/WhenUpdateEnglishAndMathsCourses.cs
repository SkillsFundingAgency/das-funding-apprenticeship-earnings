using AutoFixture;
using FluentAssertions;
using Microsoft.Azure.Amqp.Framing;
using Moq;
using NUnit.Framework;
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

[TestFixture]
public class WhenUpdateEnglishAndMathsCourses
{
    private Fixture _fixture;
    private Mock<ISystemClockService> _mockSystemClockService;
    private decimal _agreedPrice;
    private DateTime _actualStartDate;
    private DateTime _plannedEndDate;

    public WhenUpdateEnglishAndMathsCourses()
    {
        _fixture = new Fixture();
    }

    [SetUp]
    public void SetUp()
    {
        _mockSystemClockService = new Mock<ISystemClockService>();
        _mockSystemClockService.Setup(x => x.UtcNow).Returns(new DateTime(2021, 8, 30));

        _agreedPrice = _fixture.Create<decimal>();
        _actualStartDate = new DateTime(2021, 1, 15);
        _plannedEndDate = new DateTime(2022, 1, 31);
    }

    [Test]
    public void UpdateMathsAndEnglishCourses_ShouldAddCoursesToEarningsProfile()
    {
        // Arrange
        var courses = new List<EnglishAndMaths>
        {
            CreateMathsAndEnglishCourse(new DateTime(2021, 2, 1), new DateTime(2021, 4, 30), 300, "M102"),
            CreateMathsAndEnglishCourse(new DateTime(2021, 5, 1), new DateTime(2021, 7, 31), 450, "M103")
        };

        var sut = CreateApprenticeship();

        // Act
        sut.UpdateEnglishAndMathsCourses(courses, _mockSystemClockService.Object);

        // Assert
        var updatedProfile = sut.Episodes.First().EarningsProfile;
        updatedProfile.MathsAndEnglishCourses.Count.Should().Be(2);
        updatedProfile.MathsAndEnglishCourses.Sum(x => x.Amount).Should().Be(750);
    }

    [Test]
    public void UpdateMathsAndEnglishCourses_ShouldNotGenerateANewEarningsProfileVersionOrArchiveTheEarningsProfile()
    {
        // Arrange
        var sut = CreateApprenticeship();
        sut.FlushEvents();
        var initialVersion = sut.Episodes.First().EarningsProfile!.Version;

        var courses = new List<EnglishAndMaths>
        {
            CreateMathsAndEnglishCourse(new DateTime(2021, 2, 1), new DateTime(2021, 3, 31), 200, "M101")
        };

        // Act
        sut.UpdateEnglishAndMathsCourses(courses, _mockSystemClockService.Object);

        // Assert
        var events = sut.FlushEvents().ToList();
        events.Should().NotContain(x => x is Types.EarningsProfileUpdatedEvent);
        events.Should().ContainSingle(x => x is Types.EarningsProfileForEnglishAndMathsUpdatedEvent);
        sut.Episodes.First().EarningsProfile!.Version.Should().Be(initialVersion);
        sut.Episodes.First().HasEarningsProfileVersionUpdate.Should().BeFalse();
    }

    [Test]
    public void UpdateMathsAndEnglishCourses_WhenCourseNoLongerSuppliedBeforeApproval_ShouldHardDeleteCourse()
    {
        // Arrange
        var sut = CreateApprenticeship(isApproved: false);
        sut.UpdateEnglishAndMathsCourses(new List<EnglishAndMaths>
        {
            CreateMathsAndEnglishCourse(new DateTime(2021, 2, 1), new DateTime(2021, 4, 30), 300, "M102"),
            CreateMathsAndEnglishCourse(new DateTime(2021, 5, 1), new DateTime(2021, 7, 31), 450, "M103")
        }, _mockSystemClockService.Object);
        var removedCourseKey = sut.Episodes.First().EarningsProfile!.MathsAndEnglishCourses.Single(x => x.LearnAimRef == "M103").Key;
        sut.FlushEvents();

        // Act
        sut.UpdateEnglishAndMathsCourses(new List<EnglishAndMaths>
        {
            CreateMathsAndEnglishCourse(new DateTime(2021, 2, 1), new DateTime(2021, 4, 30), 300, "M102")
        }, _mockSystemClockService.Object);

        // Assert
        var profile = sut.Episodes.First().EarningsProfile!;
        profile.MathsAndEnglishCourses.Should().ContainSingle(x => x.LearnAimRef == "M102" && !x.IsRemoved);
        profile.EnglishAndMathsCourseChanges.Removed.Should().BeEquivalentTo([removedCourseKey]);
        sut.FlushEvents().OfType<Types.EarningsProfileForEnglishAndMathsUpdatedEvent>().Should().BeEmpty();
    }

    [Test]
    public void UpdateMathsAndEnglishCourses_WhenOnlyTheStartDateChangesBeforeApproval_ShouldKeepTheLatestCourseAndHardDeleteThePreviousOne()
    {
        // Arrange
        var sut = CreateApprenticeship(isApproved: false);
        sut.UpdateEnglishAndMathsCourses(new List<EnglishAndMaths>
        {
            CreateMathsAndEnglishCourse(new DateTime(2021, 2, 1), new DateTime(2021, 7, 31), 300, "M102")
        }, _mockSystemClockService.Object);
        var previousCourseKey = sut.Episodes.First().EarningsProfile!.MathsAndEnglishCourses.Single().Key;

        // Act
        sut.UpdateEnglishAndMathsCourses(new List<EnglishAndMaths>
        {
            CreateMathsAndEnglishCourse(new DateTime(2021, 3, 1), new DateTime(2021, 7, 31), 300, "M102")
        }, _mockSystemClockService.Object);

        // Assert
        var profile = sut.Episodes.First().EarningsProfile!;
        var course = profile.MathsAndEnglishCourses.Should().ContainSingle().Subject;
        course.StartDate.Should().Be(new DateTime(2021, 3, 1));
        course.IsRemoved.Should().BeFalse();
        course.Key.Should().NotBe(previousCourseKey);
        profile.EnglishAndMathsCourseChanges.Removed.Should().BeEquivalentTo([previousCourseKey]);
        profile.EnglishAndMathsCourseChanges.Created.Should().BeEquivalentTo([course.Key]);
    }

    [Test]
    public void UpdateMathsAndEnglishCourses_WhenAnUnapprovedLearningHasAPreviouslyRemovedCourseAndOnlyTheStartDateChanges_ShouldLeaveOnlyTheLatestCourse()
    {
        // Arrange - a course left soft deleted on a learning which is now unapproved
        var sut = CreateApprenticeshipWithRemovedCourseThenUnapproved(new DateTime(2021, 2, 1), "M102");

        // Act
        sut.UpdateEnglishAndMathsCourses(new List<EnglishAndMaths>
        {
            CreateMathsAndEnglishCourse(new DateTime(2021, 3, 1), new DateTime(2021, 7, 31), 300, "M102")
        }, _mockSystemClockService.Object);

        // Assert
        var course = sut.Episodes.First().EarningsProfile!.MathsAndEnglishCourses.Should().ContainSingle().Subject;
        course.StartDate.Should().Be(new DateTime(2021, 3, 1));
        course.IsRemoved.Should().BeFalse();
    }

    [Test]
    public void UpdateMathsAndEnglishCourses_WhenAnUnapprovedLearningHasAPreviouslyRemovedCourseThatIsSuppliedAgain_ShouldReinstateItRatherThanCreateANewEntry()
    {
        // Arrange - a course left soft deleted on a learning which is now unapproved
        var sut = CreateApprenticeshipWithRemovedCourseThenUnapproved(new DateTime(2021, 2, 1), "M102");
        var removedCourseKey = sut.Episodes.First().EarningsProfile!.MathsAndEnglishCourses.Single().Key;

        // Act
        sut.UpdateEnglishAndMathsCourses(new List<EnglishAndMaths>
        {
            CreateMathsAndEnglishCourse(new DateTime(2021, 2, 1), new DateTime(2021, 7, 31), 300, "M102")
        }, _mockSystemClockService.Object);

        // Assert
        var course = sut.Episodes.First().EarningsProfile!.MathsAndEnglishCourses.Should().ContainSingle().Subject;
        course.Key.Should().Be(removedCourseKey);
        course.IsRemoved.Should().BeFalse();
    }

    [Test]
    public void UpdateMathsAndEnglishCourses_WhenOnlyTheStartDateChangesAfterApproval_ShouldSoftDeleteThePreviousCourse()
    {
        // Arrange
        var sut = CreateApprenticeship();
        sut.UpdateEnglishAndMathsCourses(new List<EnglishAndMaths>
        {
            CreateMathsAndEnglishCourse(new DateTime(2021, 2, 1), new DateTime(2021, 7, 31), 300, "M102")
        }, _mockSystemClockService.Object);
        var previousCourseKey = sut.Episodes.First().EarningsProfile!.MathsAndEnglishCourses.Single().Key;

        // Act
        sut.UpdateEnglishAndMathsCourses(new List<EnglishAndMaths>
        {
            CreateMathsAndEnglishCourse(new DateTime(2021, 3, 1), new DateTime(2021, 7, 31), 300, "M102")
        }, _mockSystemClockService.Object);

        // Assert
        var courses = sut.Episodes.First().EarningsProfile!.MathsAndEnglishCourses;
        courses.Should().HaveCount(2);
        courses.Single(x => x.Key == previousCourseKey).IsRemoved.Should().BeTrue();
        courses.Single(x => x.Key != previousCourseKey).Should().Match<EnglishAndMaths>(x => !x.IsRemoved && x.StartDate == new DateTime(2021, 3, 1));
    }

    [Test]
    public void UpdateMathsAndEnglishCourses_WhenNoCoursesSuppliedBeforeApproval_ShouldHardDeleteAllCourses()
    {
        // Arrange
        var sut = CreateApprenticeship(isApproved: false);
        sut.UpdateEnglishAndMathsCourses(new List<EnglishAndMaths>
        {
            CreateMathsAndEnglishCourse(new DateTime(2021, 2, 1), new DateTime(2021, 4, 30), 300, "M102"),
            CreateMathsAndEnglishCourse(new DateTime(2021, 5, 1), new DateTime(2021, 7, 31), 450, "M103")
        }, _mockSystemClockService.Object);

        // Act
        sut.UpdateEnglishAndMathsCourses(new List<EnglishAndMaths>(), _mockSystemClockService.Object);

        // Assert
        sut.Episodes.First().EarningsProfile!.MathsAndEnglishCourses.Should().BeEmpty();
    }

    [Test]
    public void UpdateMathsAndEnglishCourses_WhenCourseNoLongerSupplied_ShouldMarkCourseAsRemoved()
    {
        // Arrange
        var sut = CreateApprenticeship();
        sut.UpdateEnglishAndMathsCourses(new List<EnglishAndMaths>
        {
            CreateMathsAndEnglishCourse(new DateTime(2021, 2, 1), new DateTime(2021, 4, 30), 300, "M102"),
            CreateMathsAndEnglishCourse(new DateTime(2021, 5, 1), new DateTime(2021, 7, 31), 450, "M103")
        }, _mockSystemClockService.Object);

        // Act
        sut.UpdateEnglishAndMathsCourses(new List<EnglishAndMaths>
        {
            CreateMathsAndEnglishCourse(new DateTime(2021, 2, 1), new DateTime(2021, 4, 30), 300, "M102")
        }, _mockSystemClockService.Object);

        // Assert
        var courses = sut.Episodes.First().EarningsProfile!.MathsAndEnglishCourses;
        courses.Count.Should().Be(2);

        var retained = courses.Single(x => x.LearnAimRef == "M102");
        retained.IsRemoved.Should().BeFalse();
        retained.Instalments.Should().NotBeEmpty();

        var removed = courses.Single(x => x.LearnAimRef == "M103");
        removed.IsRemoved.Should().BeTrue();
        removed.Instalments.Should().BeEmpty();
        removed.AdditionalPayments.Should().BeEmpty();
    }

    [Test]
    public void UpdateMathsAndEnglishCourses_WhenNoCoursesSupplied_ShouldMarkAllCoursesAsRemoved()
    {
        // Arrange
        var sut = CreateApprenticeship();
        sut.UpdateEnglishAndMathsCourses(new List<EnglishAndMaths>
        {
            CreateMathsAndEnglishCourse(new DateTime(2021, 2, 1), new DateTime(2021, 4, 30), 300, "M102"),
            CreateMathsAndEnglishCourse(new DateTime(2021, 5, 1), new DateTime(2021, 7, 31), 450, "M103")
        }, _mockSystemClockService.Object);

        // Act
        sut.UpdateEnglishAndMathsCourses(new List<EnglishAndMaths>(), _mockSystemClockService.Object);

        // Assert
        var courses = sut.Episodes.First().EarningsProfile!.MathsAndEnglishCourses;
        courses.Count.Should().Be(2);
        courses.Should().OnlyContain(x => x.IsRemoved && !x.Instalments.Any());
    }

    [Test]
    public void UpdateMathsAndEnglishCourses_WhenRemovedCourseIsSuppliedAgain_ShouldReinstateCourse()
    {
        // Arrange
        var sut = CreateApprenticeship();
        sut.UpdateEnglishAndMathsCourses(new List<EnglishAndMaths>
        {
            CreateMathsAndEnglishCourse(new DateTime(2021, 2, 1), new DateTime(2021, 4, 30), 300, "M102")
        }, _mockSystemClockService.Object);
        var originalKey = sut.Episodes.First().EarningsProfile!.MathsAndEnglishCourses.Single().Key;
        sut.UpdateEnglishAndMathsCourses(new List<EnglishAndMaths>(), _mockSystemClockService.Object);

        // Act
        sut.UpdateEnglishAndMathsCourses(new List<EnglishAndMaths>
        {
            CreateMathsAndEnglishCourse(new DateTime(2021, 2, 1), new DateTime(2021, 4, 30), 300, "M102")
        }, _mockSystemClockService.Object);

        // Assert
        var course = sut.Episodes.First().EarningsProfile!.MathsAndEnglishCourses.Single();
        course.Key.Should().Be(originalKey);
        course.IsRemoved.Should().BeFalse();
        course.Instalments.Should().NotBeEmpty();
    }

    [Test]
    public void UpdateMathsAndEnglishCourses_WhenCourseAlreadyRemoved_ShouldNotGenerateNewVersion()
    {
        // Arrange
        var sut = CreateApprenticeship();
        sut.UpdateEnglishAndMathsCourses(new List<EnglishAndMaths>
        {
            CreateMathsAndEnglishCourse(new DateTime(2021, 2, 1), new DateTime(2021, 4, 30), 300, "M102")
        }, _mockSystemClockService.Object);
        sut.UpdateEnglishAndMathsCourses(new List<EnglishAndMaths>(), _mockSystemClockService.Object);
        var version = sut.Episodes.First().EarningsProfile!.Version;

        // Act
        sut.UpdateEnglishAndMathsCourses(new List<EnglishAndMaths>(), _mockSystemClockService.Object);

        // Assert
        sut.Episodes.First().EarningsProfile!.Version.Should().Be(version);
    }

    [Test]
    public void UpdateMathsAndEnglishCourses_WhenCourseAdded_ShouldRaiseEnglishAndMathsEarningsProfileUpdatedEvent()
    {
        // Arrange
        var sut = CreateApprenticeship();
        sut.FlushEvents();

        // Act
        sut.UpdateEnglishAndMathsCourses(new List<EnglishAndMaths>
        {
            CreateMathsAndEnglishCourse(new DateTime(2021, 2, 1), new DateTime(2021, 4, 30), 300, "M102")
        }, _mockSystemClockService.Object);

        // Assert
        var course = sut.Episodes.First().EarningsProfile!.MathsAndEnglishCourses.Single();
        var events = sut.FlushEvents().OfType<Types.EarningsProfileForEnglishAndMathsUpdatedEvent>().ToList();
        events.Should().ContainSingle();
        events.Single().EnglishAndMathsKey.Should().Be(course.Key);
        events.Single().Version.Should().Be(course.Version);
        events.Single().EnglishAndMaths.IsRemoved.Should().BeFalse();
    }

    [Test]
    public void UpdateMathsAndEnglishCourses_WhenCourseChanged_ShouldGenerateNewVersionForChangedCourseOnly()
    {
        // Arrange
        var sut = CreateApprenticeship();
        sut.UpdateEnglishAndMathsCourses(new List<EnglishAndMaths>
        {
            CreateMathsAndEnglishCourse(new DateTime(2021, 2, 1), new DateTime(2021, 4, 30), 300, "M102"),
            CreateMathsAndEnglishCourse(new DateTime(2021, 5, 1), new DateTime(2021, 7, 31), 450, "M103")
        }, _mockSystemClockService.Object);
        sut.FlushEvents();
        var originalCourses = sut.Episodes.First().EarningsProfile!.MathsAndEnglishCourses.ToDictionary(x => x.LearnAimRef, x => x.Version);

        // Act
        sut.UpdateEnglishAndMathsCourses(new List<EnglishAndMaths>
        {
            CreateMathsAndEnglishCourse(new DateTime(2021, 2, 1), new DateTime(2021, 4, 30), 300, "M102"),
            CreateMathsAndEnglishCourse(new DateTime(2021, 5, 1), new DateTime(2021, 7, 31), 600, "M103")
        }, _mockSystemClockService.Object);

        // Assert
        var courses = sut.Episodes.First().EarningsProfile!.MathsAndEnglishCourses;
        courses.Single(x => x.LearnAimRef == "M102").Version.Should().Be(originalCourses["M102"]);

        var changedCourse = courses.Single(x => x.LearnAimRef == "M103");
        changedCourse.Version.Should().NotBe(originalCourses["M103"]);

        var events = sut.FlushEvents().OfType<Types.EarningsProfileForEnglishAndMathsUpdatedEvent>().ToList();
        events.Should().ContainSingle();
        events.Single().EnglishAndMathsKey.Should().Be(changedCourse.Key);
        events.Single().Version.Should().Be(changedCourse.Version);
    }

    [Test]
    public void UpdateMathsAndEnglishCourses_WhenCourseRemoved_ShouldGenerateNewVersionAndRaiseEvent()
    {
        // Arrange
        var sut = CreateApprenticeship();
        sut.UpdateEnglishAndMathsCourses(new List<EnglishAndMaths>
        {
            CreateMathsAndEnglishCourse(new DateTime(2021, 2, 1), new DateTime(2021, 4, 30), 300, "M102")
        }, _mockSystemClockService.Object);
        sut.FlushEvents();
        var originalVersion = sut.Episodes.First().EarningsProfile!.MathsAndEnglishCourses.Single().Version;

        // Act
        sut.UpdateEnglishAndMathsCourses(new List<EnglishAndMaths>(), _mockSystemClockService.Object);

        // Assert
        var course = sut.Episodes.First().EarningsProfile!.MathsAndEnglishCourses.Single();
        course.Version.Should().NotBe(originalVersion);

        var events = sut.FlushEvents().OfType<Types.EarningsProfileForEnglishAndMathsUpdatedEvent>().ToList();
        events.Should().ContainSingle();
        events.Single().Version.Should().Be(course.Version);
        events.Single().EnglishAndMaths.IsRemoved.Should().BeTrue();
        events.Single().EnglishAndMaths.Instalments.Should().BeEmpty();
    }

    [Test]
    public void UpdateMathsAndEnglishCourses_WhenCourseChangedMultipleTimes_ShouldOnlyKeepLatestEventForCourse()
    {
        // Arrange
        var sut = CreateApprenticeship();
        sut.UpdateEnglishAndMathsCourses(new List<EnglishAndMaths>
        {
            CreateMathsAndEnglishCourse(new DateTime(2021, 2, 1), new DateTime(2021, 4, 30), 300, "M102")
        }, _mockSystemClockService.Object);

        // Act
        sut.UpdateEnglishAndMathsCourses(new List<EnglishAndMaths>
        {
            CreateMathsAndEnglishCourse(new DateTime(2021, 2, 1), new DateTime(2021, 4, 30), 400, "M102")
        }, _mockSystemClockService.Object);

        // Assert
        var course = sut.Episodes.First().EarningsProfile!.MathsAndEnglishCourses.Single();
        var events = sut.FlushEvents().OfType<Types.EarningsProfileForEnglishAndMathsUpdatedEvent>().ToList();
        events.Should().ContainSingle();
        events.Single().Version.Should().Be(course.Version);
        events.Single().EnglishAndMaths.Amount.Should().Be(400);
    }

    [Test]
    public void UpdateMathsAndEnglishCourses_ShouldRecordCreatedChangedRemovedAndReinstatedCourseKeys()
    {
        // Arrange
        var sut = CreateApprenticeship();
        sut.UpdateEnglishAndMathsCourses(new List<EnglishAndMaths>
        {
            CreateMathsAndEnglishCourse(new DateTime(2021, 2, 1), new DateTime(2021, 4, 30), 300, "M101"),
            CreateMathsAndEnglishCourse(new DateTime(2021, 2, 1), new DateTime(2021, 4, 30), 300, "M102"),
            CreateMathsAndEnglishCourse(new DateTime(2021, 5, 1), new DateTime(2021, 7, 31), 450, "M103")
        }, _mockSystemClockService.Object);
        sut.UpdateEnglishAndMathsCourses(new List<EnglishAndMaths>
        {
            CreateMathsAndEnglishCourse(new DateTime(2021, 2, 1), new DateTime(2021, 4, 30), 300, "M101"),
            CreateMathsAndEnglishCourse(new DateTime(2021, 2, 1), new DateTime(2021, 4, 30), 300, "M102")
        }, _mockSystemClockService.Object);
        var keys = sut.Episodes.First().EarningsProfile!.MathsAndEnglishCourses.ToDictionary(x => x.LearnAimRef, x => x.Key);

        // Act
        sut.UpdateEnglishAndMathsCourses(new List<EnglishAndMaths>
        {
            CreateMathsAndEnglishCourse(new DateTime(2021, 2, 1), new DateTime(2021, 4, 30), 350, "M102"),
            CreateMathsAndEnglishCourse(new DateTime(2021, 5, 1), new DateTime(2021, 7, 31), 450, "M103"),
            CreateMathsAndEnglishCourse(new DateTime(2021, 6, 1), new DateTime(2021, 7, 31), 200, "M104")
        }, _mockSystemClockService.Object);

        // Assert
        var profile = sut.Episodes.First().EarningsProfile!;
        var changes = profile.EnglishAndMathsCourseChanges;
        changes.Created.Should().BeEquivalentTo(new[] { profile.MathsAndEnglishCourses.Single(x => x.LearnAimRef == "M104").Key });
        changes.Changed.Should().BeEquivalentTo(new[] { keys["M102"] });
        changes.Removed.Should().BeEquivalentTo(new[] { keys["M101"] });
        changes.Reinstated.Should().BeEquivalentTo(new[] { keys["M103"] });
    }

    [Test]
    public void UpdateMathsAndEnglishCourses_WhenNothingChanges_ShouldRecordNoCourseKeys()
    {
        // Arrange
        var sut = CreateApprenticeship();
        sut.UpdateEnglishAndMathsCourses(new List<EnglishAndMaths>
        {
            CreateMathsAndEnglishCourse(new DateTime(2021, 2, 1), new DateTime(2021, 4, 30), 300, "M102")
        }, _mockSystemClockService.Object);

        // Act
        sut.UpdateEnglishAndMathsCourses(new List<EnglishAndMaths>
        {
            CreateMathsAndEnglishCourse(new DateTime(2021, 2, 1), new DateTime(2021, 4, 30), 300, "M102")
        }, _mockSystemClockService.Object);

        // Assert
        sut.Episodes.First().EarningsProfile!.EnglishAndMathsCourseChanges.HasChanges.Should().BeFalse();
    }

    private ApprenticeshipLearning CreateApprenticeship(bool isApproved = true)
    {
        var sut = _fixture.CreateLearningWithApprenticeship(_actualStartDate, _plannedEndDate, _agreedPrice);
        sut.Calculate(_mockSystemClockService.Object, string.Empty, initialGenerationIsApproved: isApproved);
        return sut;
    }

    private ApprenticeshipLearning CreateApprenticeshipWithRemovedCourseThenUnapproved(DateTime courseStartDate, string learnAimRef)
    {
        var sut = CreateApprenticeship();
        sut.UpdateEnglishAndMathsCourses(new List<EnglishAndMaths>
        {
            CreateMathsAndEnglishCourse(courseStartDate, new DateTime(2021, 7, 31), 300, learnAimRef)
        }, _mockSystemClockService.Object);
        sut.UpdateEnglishAndMathsCourses(new List<EnglishAndMaths>(), _mockSystemClockService.Object);
        sut.Calculate(_mockSystemClockService.Object, string.Empty, initialGenerationIsApproved: false);

        var profile = sut.Episodes.First().EarningsProfile!;
        profile.IsApproved.Should().BeFalse();
        profile.MathsAndEnglishCourses.Should().ContainSingle(x => x.IsRemoved);

        return sut;
    }

    private EnglishAndMaths CreateMathsAndEnglishCourse(DateTime startDate, DateTime endDate, decimal amount, string courseCode)
    {
        var periodInLearning = PeriodInLearningHelper.Create(startDate, endDate, endDate);

        return new EnglishAndMaths(
            startDate,
            endDate,
            courseCode,
            courseCode,
            amount,
            null,
            null,
            null,
            null,
            [periodInLearning]);
    }
}