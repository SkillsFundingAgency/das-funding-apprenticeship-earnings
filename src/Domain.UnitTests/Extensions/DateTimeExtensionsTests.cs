using FluentAssertions;
using NUnit.Framework;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.Domain.UnitTests.Extensions;

[TestFixture]
public class DateTimeExtensionsTests
{
    [Test]
    public void SameMonth_NoMonthEnd_ShouldReturnZero()
    {
        var start = new DateTime(2024, 5, 10);
        var end = new DateTime(2024, 5, 20);

        var result = start.NumberOfCensusDates(end);

        result.Should().Be(0);
    }

    [Test]
    public void StartAndEndOnSameMonthEnd_ShouldReturnOne()
    {
        var start = new DateTime(2024, 5, 31);
        var end = new DateTime(2024, 5, 31);

        var result = start.NumberOfCensusDates(end);

        result.Should().Be(1);
    }

    [Test]
    public void RangeCrossingOneMonthEnd_ShouldReturnOne()
    {
        var start = new DateTime(2024, 5, 20);
        var end = new DateTime(2024, 6, 5);

        var result = start.NumberOfCensusDates(end);

        result.Should().Be(1);
    }

    [Test]
    public void RangeCrossingMultipleMonthEnds_ShouldReturnCorrectCount()
    {
        var start = new DateTime(2024, 2, 10);
        var end = new DateTime(2024, 5, 2);

        var result = start.NumberOfCensusDates(end);

        result.Should().Be(3);
    }

    [Test]
    public void HandlesLeapYearFebruaryCorrectly()
    {
        var start = new DateTime(2024, 2, 1);
        var end = new DateTime(2024, 3, 1);

        var result = start.NumberOfCensusDates(end);

        result.Should().Be(1);
    }

    [Test]
    public void RangeEndingBeforeMonthEnd_ShouldNotCountEndOfMonth()
    {
        var start = new DateTime(2024, 4, 1);
        var end = new DateTime(2024, 4, 29);

        var result = start.NumberOfCensusDates(end);

        result.Should().Be(0);
    }

    [Test]
    public void ReversedDates_Throws()
    {
        var start = new DateTime(2024, 7, 20);
        var end = new DateTime(2024, 5, 20);

        Assert.Throws<ArgumentException>(() => start.NumberOfCensusDates(end));
    }

    [Test]
    public void FullYear_ShouldReturnTwelve()
    {
        var start = new DateTime(2023, 1, 1);
        var end = new DateTime(2023, 12, 31);

        var result = start.NumberOfCensusDates(end);

        result.Should().Be(12);
    }

    [Test]
    public void PartialYear_ShouldCountOnlyIncludedMonthEnds()
    {
        var start = new DateTime(2023, 3, 15);
        var end = new DateTime(2023, 8, 2);

        var result = start.NumberOfCensusDates(end);

        result.Should().Be(5);
    }

    [Test]
    public void ExactMonthEndsAcrossSeveralMonths_ShouldCountEach()
    {
        var start = new DateTime(2024, 1, 31);
        var end = new DateTime(2024, 4, 30);

        var result = start.NumberOfCensusDates(end);

        result.Should().Be(4);
    }
}
