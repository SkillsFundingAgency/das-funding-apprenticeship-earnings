using System;
using System.Linq;
using FluentAssertions;
using NUnit.Framework;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Apprenticeship;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Calculations;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Extensions;

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
        var result = MathsAndEnglishPayments.GenerateMathsAndEnglishPayments(
            new GenerateMathsAndEnglishPaymentsCommand(startDate, endDate, "M101", 300)
        );

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
        var result = MathsAndEnglishPayments.GenerateMathsAndEnglishPayments(
            new GenerateMathsAndEnglishPaymentsCommand(startDate, endDate, "E102", 300)
        );

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
        var result = MathsAndEnglishPayments.GenerateMathsAndEnglishPayments(
            new GenerateMathsAndEnglishPaymentsCommand(startDate, endDate, "E103", 200)
        );

        // Assert
        result.Instalments.Count.Should().Be(2);
        result.Instalments.First().Amount.Should().Be(100);
        result.Instalments.ToList()[1].Amount.Should().Be(100);
    }

    [Test]
    public void GenerateMathsAndEnglishPayments_ShouldCreateOneInstalmentForCoursesWhichSpanNoCensusDate()
    {
        // Arrange
        var startDate = new DateTime(2024, 02, 1);
        var endDate = new DateTime(2024, 02, 26);

        // Act
        var result = MathsAndEnglishPayments.GenerateMathsAndEnglishPayments(
            new GenerateMathsAndEnglishPaymentsCommand(startDate, endDate, "E102", 931)
        );

        // Assert
        result.Instalments.Count.Should().Be(1);
        result.Instalments.Single().Amount.Should().Be(931);
        result.Instalments.Single().AcademicYear.Should().Be(2324);
        result.Instalments.Single().DeliveryPeriod.Should().Be(7);
    }

    [Test]
    public void GenerateMathsAndEnglishPayments_ShouldNotHardDeleteInstalmentsAfterTheWithdrawalDate()
    {
        // Arrange
        var startDate = new DateTime(2023, 1, 1);
        var endDate = new DateTime(2023, 12, 31);
        var withdrawalDate = new DateTime(2023, 6, 15);

        // Act
        var result = MathsAndEnglishPayments.GenerateMathsAndEnglishPayments(new GenerateMathsAndEnglishPaymentsCommand(startDate, endDate, "E102", 300, withdrawalDate: withdrawalDate));

        // Assert
        result.Instalments.Count.Should().Be(12);
    }

    [TestCase(168, 42, true)]
    [TestCase(168, 41, false)]
    [TestCase(14, 14, true)]
    [TestCase(14, 13, false)]
    [TestCase(14, 14, true)]
    [TestCase(13, 1, true)]
    [TestCase(13, 0, false)]
    public void GenerateMathsAndEnglishPayments_ShouldNotReturnAnyInstalmentsIfWithdrawnBeforeQualifyingPeriod(int plannedDuration, int actualDuration, bool expectedToQualifyAfterWithdrawal)
    {
        // Arrange
        var startDate = new DateTime(2023, 1, 31);
        var endDate = startDate.AddDays(plannedDuration - 1);
        var withdrawalDate = startDate.AddDays(actualDuration - 1);

        // Act
        var result = MathsAndEnglishPayments.GenerateMathsAndEnglishPayments(new GenerateMathsAndEnglishPaymentsCommand(startDate, endDate, "E102", 300, withdrawalDate:withdrawalDate));

        // Assert
        result.Instalments.Any().Should().Be(expectedToQualifyAfterWithdrawal);
    }

    [TestCase(20)]
    [TestCase(93)]
    [TestCase(100)]
    [TestCase(130)]
    public void GenerateMathsAndEnglishPayments_ShouldAdjustAmountForPriorLearning(int priorLearningAdjustmentPercentage)
    {
        // Arrange
        var startDate = new DateTime(2023, 8, 1);
        var endDate = new DateTime(2023, 12, 31);
        var expectedAdjustedAmount = 211.6m * priorLearningAdjustmentPercentage / 100m;

        // Act
        var result = MathsAndEnglishPayments.GenerateMathsAndEnglishPayments(new GenerateMathsAndEnglishPaymentsCommand(startDate, endDate, "E102", 1058, null, null, priorLearningAdjustmentPercentage));

        // Assert
        result.Instalments.Count.Should().Be(5);
        result.Instalments.Should().AllSatisfy(x => x.Amount.Should().Be(expectedAdjustedAmount));
    }

    [TestCase(0)]
    [TestCase(null)]
    public void GenerateMathsAndEnglishPayments_ShouldNotAdjustAmountForPriorLearningWhenNullOrZero(int? priorLearningAdjustmentPercentage)
    {
        // Arrange
        var startDate = new DateTime(2023, 8, 1);
        var endDate = new DateTime(2023, 12, 31);
        var expectedUnAdjustedAmount = 211.6m;

        // Act
        var result = MathsAndEnglishPayments.GenerateMathsAndEnglishPayments(new GenerateMathsAndEnglishPaymentsCommand(startDate, endDate, "E102", 1058, null, null, priorLearningAdjustmentPercentage));

        // Assert
        result.Instalments.Count.Should().Be(5);
        result.Instalments.Should().AllSatisfy(x => x.Amount.Should().Be(expectedUnAdjustedAmount));
    }

    [Test]
    public void GenerateMathsAndEnglishPayments_ShouldAdjustForCompletionWithABalancingPayment()
    {
        // Arrange
        var startDate = new DateTime(2023, 10, 1);
        var endDate = new DateTime(2024, 3, 31);
        var actualEndDate = new DateTime(2023, 12, 31);

        // Act
        var result = MathsAndEnglishPayments.GenerateMathsAndEnglishPayments(
            new GenerateMathsAndEnglishPaymentsCommand(startDate, endDate, "E102", 300, actualEndDate:actualEndDate)
        );

        // Assert
        result.Instalments.Count.Should().Be(3);
        result.Instalments.Where(x => x.DeliveryPeriod < 5).Should().AllSatisfy(x => x.Amount.Should().Be(50));
        result.Instalments.Where(x => x.DeliveryPeriod < 5).Should().AllSatisfy(x => x.Type.Should().Be(MathsAndEnglishInstalmentType.Regular));
        result.Instalments.Single(x => x.DeliveryPeriod == 5).Amount.Should().Be(200);
        result.Instalments.Single(x => x.DeliveryPeriod == 5).Type.Should().Be(MathsAndEnglishInstalmentType.Balancing);

        result.ActualEndDate.Should().Be(actualEndDate);
    }

}