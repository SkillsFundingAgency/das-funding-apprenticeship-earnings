using System;
using System.Linq;
using FluentAssertions;
using NUnit.Framework;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Calculations;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.Domain.UnitTests.Calculations;

[TestFixture]
public class MathsAndEnglishPaymentsTests
{
    [Test]
    public void GenerateMathsAndEnglishPayments_ShouldReturnEmptyInstalments_WhenDatesInvalid()
    {
        // Arrange
        var startDate = new DateTime(2023, 12, 1);
        var endDate = new DateTime(2023, 11, 30);

        // Act
        var result = MathsAndEnglishPayments.GenerateMathsAndEnglishPayments(startDate, endDate, "M101", 300);

        // Assert
        result.Should().NotBeNull();
        result.Instalments.Should().BeEmpty();
    }

    [Test]
    public void GenerateMathsAndEnglishPayments_ShouldCreateEqualInstalmentsForEachMonthSpanned()
    {
        // Arrange
        var startDate = new DateTime(2023, 10, 1);
        var endDate = new DateTime(2023, 12, 31);

        // Act
        var result = MathsAndEnglishPayments.GenerateMathsAndEnglishPayments(startDate, endDate, "E102", 300);

        // Assert
        result.Instalments.Count.Should().Be(3);
        result.Instalments.First().Amount.Should().Be(100);
        result.Instalments.ToList()[2].AcademicYear.Should().Be(endDate.ToAcademicYear());
    }

    [Test]
    public void GenerateMathsAndEnglishPayments_ShouldGenerateInstalments_WhenStartDateIsLastDayOfMonth()
    {
        // Arrange
        var startDate = new DateTime(2023, 11, 30);
        var endDate = new DateTime(2023, 12, 31);

        // Act
        var result = MathsAndEnglishPayments.GenerateMathsAndEnglishPayments(startDate, endDate, "E103", 200);

        // Assert
        result.Instalments.Count.Should().Be(2);
        result.Instalments.First().Amount.Should().Be(100);
        result.Instalments.ToList()[1].Amount.Should().Be(100);
    }
}