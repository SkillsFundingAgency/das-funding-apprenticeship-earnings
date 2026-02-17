using System;
using FluentAssertions;
using NUnit.Framework;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Calculations;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Extensions;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.Domain.UnitTests.Calculations;

[TestFixture]
public class ShortCoursePaymentsTests
{
    [Test]
    public void GenerateShortCoursePayments_ShouldGenerateTwoPayments()
    {
        // Arrange
        var totalPrice = 1000m;
        var startDate = new DateTime(2023, 11, 1);
        var endDate = new DateTime(2023, 11, 30);
        var priceKey = Guid.NewGuid();

        // Act
        var result = ShortCoursePayments.GenerateShortCoursePayments(totalPrice, startDate, endDate, priceKey);

        // Assert
        result.Count.Should().Be(2);
    }

    [Test]
    public void GenerateShortCoursePayments_ShouldGenerateCorrectPaymentAmounts()
    {
        // Arrange
        var totalPrice = 1000m;
        var startDate = new DateTime(2023, 11, 1);
        var endDate = new DateTime(2023, 11, 30);
        var priceKey = Guid.NewGuid();

        // Act
        var result = ShortCoursePayments.GenerateShortCoursePayments(totalPrice, startDate, endDate, priceKey);

        // Assert
        result[0].Amount.Should().Be(300m);
        result[1].Amount.Should().Be(700m);
    }

    [Test]
    public void GenerateShortCoursePayments_ShouldGeneratePaymentsWithCorrectPriceKey()
    {
        // Arrange
        var totalPrice = 1000m;
        var startDate = new DateTime(2023, 11, 1);
        var endDate = new DateTime(2023, 11, 30);
        var priceKey = Guid.NewGuid();

        // Act
        var result = ShortCoursePayments.GenerateShortCoursePayments(totalPrice, startDate, endDate, priceKey);

        // Assert
        result[0].EpisodePriceKey.Should().Be(priceKey);
        result[1].EpisodePriceKey.Should().Be(priceKey);
    }

    [Test]
    public void GenerateShortCoursePayments_ShouldSetFirstPaymentAcademicYearAndDeliveryPeriodBasedOnThirtyPercentPoint()
    {
        // Arrange
        var totalPrice = 1000m;
        var startDate = new DateTime(2023, 11, 1);
        var endDate = new DateTime(2023, 11, 30);
        var priceKey = Guid.NewGuid();

        var duration = (endDate - startDate).Days + 1;
        var firstPaymentDate = startDate.AddDays(Math.Floor(duration * 0.3) - 1);

        // Act
        var result = ShortCoursePayments.GenerateShortCoursePayments(totalPrice, startDate, endDate, priceKey);

        // Assert
        result[0].AcademicYear.Should().Be(firstPaymentDate.ToAcademicYear());
        result[0].DeliveryPeriod.Should().Be(firstPaymentDate.ToDeliveryPeriod());
    }

    [Test]
    public void GenerateShortCoursePayments_ShouldSetSecondPaymentAcademicYearAndDeliveryPeriodBasedOnEndDate()
    {
        // Arrange
        var totalPrice = 1000m;
        var startDate = new DateTime(2023, 11, 1);
        var endDate = new DateTime(2023, 11, 30);
        var priceKey = Guid.NewGuid();

        // Act
        var result = ShortCoursePayments.GenerateShortCoursePayments(totalPrice, startDate, endDate, priceKey);

        // Assert
        result[1].AcademicYear.Should().Be(endDate.ToAcademicYear());
        result[1].DeliveryPeriod.Should().Be(endDate.ToDeliveryPeriod());
    }

    [Test]
    public void GenerateShortCoursePayments_ShouldHandleSingleDayCourse()
    {
        // Arrange
        var totalPrice = 1000m;
        var startDate = new DateTime(2023, 11, 15);
        var endDate = new DateTime(2023, 11, 15);
        var priceKey = Guid.NewGuid();

        // Act
        var result = ShortCoursePayments.GenerateShortCoursePayments(totalPrice, startDate, endDate, priceKey);

        // Assert
        result.Count.Should().Be(2);
        result[0].Amount.Should().Be(300m);
        result[1].Amount.Should().Be(700m);
    }
}