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
public class WhenUpdateMathsAndEnglishCourses
{
    private Fixture _fixture;
    private Mock<ISystemClockService> _mockSystemClockService;
    private decimal _agreedPrice;
    private DateTime _actualStartDate;
    private DateTime _plannedEndDate;

    public WhenUpdateMathsAndEnglishCourses()
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
        var courses = new List<MathsAndEnglish>
        {
            new(new DateTime(2021, 2, 1), new DateTime(2021, 4, 30), "M102", 300, new List < MathsAndEnglishInstalment >(), null, null, null, null),
            new(new DateTime(2021, 5, 1), new DateTime(2021, 7, 31), "M103", 450, new List < MathsAndEnglishInstalment >(), null, null, null, null)
        };

        var sut = CreateApprenticeship();

        // Act
        sut.UpdateMathsAndEnglishCourses(courses, _mockSystemClockService.Object);

        // Assert
        var updatedProfile = sut.ApprenticeshipEpisodes.First().EarningsProfile;
        updatedProfile.MathsAndEnglishCourses.Count.Should().Be(2);
        updatedProfile.MathsAndEnglishCourses.Sum(x => x.Amount).Should().Be(750);
    }

    [Test]
    public void UpdateMathsAndEnglishCourses_ShouldRaiseEarningsProfileArchivedEvent()
    {
        // Arrange
        var sut = CreateApprenticeship();
        sut.UpdateMathsAndEnglishCourses(new List<MathsAndEnglish>(), _mockSystemClockService.Object); // first update

        var courses = new List<MathsAndEnglish>
        {
            new(new DateTime(2021, 2, 1), new DateTime(2021, 3, 31), "M101", 200, new List<MathsAndEnglishInstalment>(), null, null, null, null)
        };

        // Act
        sut.UpdateMathsAndEnglishCourses(courses, _mockSystemClockService.Object);

        // Assert
        var events = sut.FlushEvents().ToList();
        events.Any(x => x is Types.EarningsProfileUpdatedEvent).Should().BeTrue();
    }

    private Apprenticeship.Apprenticeship CreateApprenticeship()
    {
        var sut = _fixture.CreateApprenticeship(_actualStartDate, _plannedEndDate, _agreedPrice);
        sut.CalculateEarnings(_mockSystemClockService.Object);
        return sut;
    }
}