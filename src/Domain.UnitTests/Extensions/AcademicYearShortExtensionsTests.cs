using System;
using FluentAssertions;
using NUnit.Framework;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Extensions;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.Domain.UnitTests.Extensions;

[TestFixture]
public class AcademicYearShortExtensionsTests
{
    [TestCase(2324, 2023, 8, 1, 2024, 7, 31)]
    [TestCase(2425, 2024, 8, 1, 2025, 7, 31)]
    public void GetAcademicYear_ReturnsAcademicYearDates(short academicYear,
        int expectedStartYear, int expectedStartMonth, int expectedStartDay,
        int expectedEndYear, int expectedEndMonth, int expectedEndDay)
    {
        var result = academicYear.GetAcademicYear();

        result.StartDate.Should().Be(new DateTime(expectedStartYear, expectedStartMonth, expectedStartDay));
        result.EndDate.Should().Be(new DateTime(expectedEndYear, expectedEndMonth, expectedEndDay));
    }
}
