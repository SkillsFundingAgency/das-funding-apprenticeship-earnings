using FluentAssertions;
using NUnit.Framework;
using SFA.DAS.Funding.ApprenticeshipEarnings.DataAccess.Entities;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Apprenticeship;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Calculations;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Extensions;
using System;
using System.Linq;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.Domain.UnitTests.Calculations;

[TestFixture]
public class EnglishAndMathsPaymentsTests
{
    [Test]
    public void GenerateMathsAndEnglishPayments_ShouldReturnEmptyInstalments_WhenDatesInvalid()
    {
        // Arrange
        var course = CreateEnglishAndMathsCourse(
            new DateTime(2023, 12, 1),
            new DateTime(2023, 11, 30)
        );

        // Act
        var result = EnglishAndMathsPayments.GenerateInstalments(course);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }

    [Test]
    public void GenerateMathsAndEnglishPayments_ShouldCreateEqualInstalmentsForEachMonthSpanned()
    {
        // Arrange
        var startDate = new DateTime(2023, 10, 1);
        var endDate = new DateTime(2023, 12, 31);
        var course = CreateEnglishAndMathsCourse(startDate, endDate, "E102", 300);

        // Act
        var result = EnglishAndMathsPayments.GenerateInstalments(course);

        // Assert
        result.Count.Should().Be(3);
        result.First().Amount.Should().Be(100);
        result.ToList()[2].AcademicYear.Should().Be(endDate.ToAcademicYear());
    }

    [Test]
    public void GenerateMathsAndEnglishPayments_ShouldGenerateInstalments_WhenStartDateIsLastDayOfMonth()
    {
        // Arrange
        var startDate = new DateTime(2023, 11, 30);
        var endDate = new DateTime(2023, 12, 31);
        var course = CreateEnglishAndMathsCourse(startDate, endDate, "E103", 200);

        // Act
        var result = EnglishAndMathsPayments.GenerateInstalments(course);

        // Assert
        result.Count.Should().Be(2);
        result.First().Amount.Should().Be(100);
        result.ToList()[1].Amount.Should().Be(100);
    }

    [Test]
    public void GenerateMathsAndEnglishPayments_ShouldCreateOneInstalmentForCoursesWhichSpanNoCensusDate()
    {
        // Arrange
        var startDate = new DateTime(2024, 02, 1);
        var endDate = new DateTime(2024, 02, 26);
        var course = CreateEnglishAndMathsCourse(startDate, endDate, "E102", 931);

        // Act
        var result = EnglishAndMathsPayments.GenerateInstalments(course);

        // Assert
        result.Count.Should().Be(1);
        result.Single().Amount.Should().Be(931);
        result.Single().AcademicYear.Should().Be(2324);
        result.Single().DeliveryPeriod.Should().Be(7);
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
        var course = CreateEnglishAndMathsCourse(startDate, endDate, "E102", 300, withdrawalDate);

        // Act
        var result = EnglishAndMathsPayments.GenerateInstalments(course);

        // Assert
        result.Any().Should().Be(expectedToQualifyAfterWithdrawal);
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
        var course = CreateEnglishAndMathsCourse(startDate, endDate, "E102", 1058, null, priorLearningAdjustmentPercentage);

        // Act
        var result = EnglishAndMathsPayments.GenerateInstalments(course);

        // Assert
        result.Count.Should().Be(5);
        result.Should().AllSatisfy(x => x.Amount.Should().Be(expectedAdjustedAmount));
    }

    [TestCase(0)]
    [TestCase(null)]
    public void GenerateMathsAndEnglishPayments_ShouldNotAdjustAmountForPriorLearningWhenNullOrZero(int? priorLearningAdjustmentPercentage)
    {
        // Arrange
        var startDate = new DateTime(2023, 8, 1);
        var endDate = new DateTime(2023, 12, 31);
        var expectedUnAdjustedAmount = 211.6m;
        var course = CreateEnglishAndMathsCourse(startDate, endDate, "E102", 1058, null, priorLearningAdjustmentPercentage);

        // Act
        var result = EnglishAndMathsPayments.GenerateInstalments(course);

        // Assert
        result.Count.Should().Be(5);
        result.Should().AllSatisfy(x => x.Amount.Should().Be(expectedUnAdjustedAmount));
    }

    [Test]
    public void GenerateMathsAndEnglishPayments_ShouldAdjustForCompletionWithABalancingPayment()
    {
        // Arrange
        var startDate = new DateTime(2023, 10, 1);
        var endDate = new DateTime(2024, 3, 31);
        var completionDate = new DateTime(2023, 12, 31);
        var course = CreateEnglishAndMathsCourse(startDate, endDate, "E102", 300, null, null, completionDate);

        // Act
        var result = EnglishAndMathsPayments.GenerateInstalments(course);

        // Assert
        result.Count.Should().Be(3);
        result.Where(x => x.DeliveryPeriod < 5).Should().AllSatisfy(x => x.Amount.Should().Be(50));
        result.Where(x => x.DeliveryPeriod < 5).Should().AllSatisfy(x => x.Type.Should().Be(MathsAndEnglishInstalmentType.Regular.ToString()));
        result.Single(x => x.DeliveryPeriod == 5).Amount.Should().Be(200);
        result.Single(x => x.DeliveryPeriod == 5).Type.Should().Be(MathsAndEnglishInstalmentType.Balancing.ToString());

    }

    private MathsAndEnglish CreateEnglishAndMathsCourse(DateTime startDate, DateTime endDate, string courseCode = "M101", decimal amount = 300, DateTime? withdrawalDate = null, int? priorLearningAdjustmentPercentage = null, DateTime? completionDate = null)
    {
        var model = new MathsAndEnglishModel
        {
            Key = Guid.NewGuid(),
            Course = courseCode,
            LearnAimRef = courseCode,
            StartDate = startDate,
            EndDate = endDate,
            Amount = amount,
            WithdrawalDate = withdrawalDate,
            CompletionDate = completionDate,
            PriorLearningAdjustmentPercentage = priorLearningAdjustmentPercentage,
            PeriodsInLearning = [ new MathsAndEnglishPeriodInLearningModel
            {
                StartDate = startDate,
                EndDate = endDate,
                OriginalExpectedEndDate = endDate,
            } ]
        };

        return MathsAndEnglish.Get(model);
    }
}