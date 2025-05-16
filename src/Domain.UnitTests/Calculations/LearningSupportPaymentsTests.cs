using FluentAssertions;
using NUnit.Framework;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Calculations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.Domain.UnitTests.Calculations;

[TestFixture]
public class LearningSupportPaymentsTests
{
    [Test]
    public void GenerateLearningSupportPayments_ShouldReturnEmptyList_WhenStartDateIsAfterEndDate()
    {
        // Arrange
        var startDate = new DateTime(2023, 12, 1);
        var endDate = new DateTime(2023, 11, 30);

        // Act
        var result = LearningSupportPayments.GenerateLearningSupportPayments(startDate, endDate);

        // Assert
        result.Should().BeEmpty();
    }

    [Test]
    public void GenerateLearningSupportPayments_ShouldReturnCorrectPayments_WhenStartDateAndEndDateAreInSameMonth()
    {
        // Arrange
        var startDate = new DateTime(2023, 11, 1);
        var endDate = new DateTime(2023, 11, 30);

        // Act
        var result = LearningSupportPayments.GenerateLearningSupportPayments(startDate, endDate);

        // Assert
        result.Count().Should().Be(1);

        result[0].AcademicYear.Should().Be(startDate.ToAcademicYear());
        result[0].DeliveryPeriod.Should().Be(startDate.ToDeliveryPeriod());
        result[0].Amount.Should().Be(150);
        result[0].DueDate.Should().Be(endDate);
        result[0].AdditionalPaymentType.Should().Be(InstalmentTypes.LearningSupport);
    }

    [Test]
    public void GenerateLearningSupportPayments_ShouldGeneratePaymentsForEachMonth_WhenStartDateAndEndDateSpanMultipleMonths()
    {
        // Arrange
        var startDate = new DateTime(2023, 10, 1);
        var endDate = new DateTime(2023, 12, 31);

        // Act
        var result = LearningSupportPayments.GenerateLearningSupportPayments(startDate, endDate);

        // Assert
        result.Count.Should().Be(3);
        result[0].DueDate.Should().Be(new DateTime(2023, 10, 31));
        result[1].DueDate.Should().Be(new DateTime(2023, 11, 30));
        result[2].DueDate.Should().Be(new DateTime(2023, 12, 31));
    }

    [Test]
    public void GenerateLearningSupportPayments_ShouldHandleEdgeCase_WhenStartDateIsOnLastDayOfMonth()
    {
        // Arrange
        var startDate = new DateTime(2023, 11, 30);
        var endDate = new DateTime(2023, 12, 31);

        // Act
        var result = LearningSupportPayments.GenerateLearningSupportPayments(startDate, endDate);

        // Assert
        result.Count.Should().Be(2);
        result[0].DueDate.Should().Be(new DateTime(2023, 11, 30));
        result[1].DueDate.Should().Be(new DateTime(2023, 12, 31));
    }

    [Test]
    public void GenerateLearningSupportPayments_ShouldReturnEmptyList_WhenStartDateAndEndDateAreSameButNotCensusDate()
    {
        // Arrange
        var startDate = new DateTime(2023, 11, 15);
        var endDate = new DateTime(2023, 11, 15);

        // Act
        var result = LearningSupportPayments.GenerateLearningSupportPayments(startDate, endDate);

        // Assert
        result.Should().BeEmpty();
    }
}
