using System;
using System.Collections.Generic;
using FluentAssertions;
using NUnit.Framework;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.Domain.UnitTests;

public class InstallmentsGenerator_GenerateTests
{
    private InstalmentsGenerator _sut;

    [SetUp]
    public void SetUp()
    {
        _sut = new InstalmentsGenerator();
    }

    [TestCase(12000, 2010, 01, 05, 2011, 12, 04, 24)]
    [TestCase(12000, 2010, 01, 05, 2011, 12, 06, 24)]
    public void ShouldGenerateCorrectNumberOfInstallments(decimal total, int startYear, int startMonth, int startDay,
        int endYear, int endMonth, int endDay, int expectedNumberOfInstallments)
    {
        var startDate = new DateTime(startYear, startMonth, startDay);
        var endDate = new DateTime(endYear, endMonth, endDay);

        var actualInstallments = _sut.Generate(total, startDate, endDate);

        actualInstallments.Should().HaveCount(expectedNumberOfInstallments);
    }

    [TestCase(12000, 2010, 01, 05, 2011, 12, 04, 500)]
    public void ShouldGenerateCorrectMonthlyInstallmentAmount(decimal total, int startYear, int startMonth,
        int startDay, int endYear, int endMonth, int endDay, decimal expectedInstallmentAmount)
    {
        var startDate = new DateTime(startYear, startMonth, startDay);
        var endDate = new DateTime(endYear, endMonth, endDay);

        var actualInstallments = _sut.Generate(total, startDate, endDate);

        actualInstallments.Should().OnlyContain(x => x.Amount == expectedInstallmentAmount);
    }

    [Test]
    public void ShouldGenerateCorrectAcademicYearsAndDeliveryPeriods()
    {
        var startDate = new DateTime(2018, 7, 10);
        var endDate = new DateTime(2019, 1, 5);

        var actualInstallments = _sut.Generate(15000, startDate, endDate);

        var expectedDeliveryPeriods = new List<(short academicYear, int deliveryPeriod)>
        {
            (1718,12),
            (1819,1),
            (1819,2),
            (1819,3),
            (1819,4),
            (1819,5),
            (1819,6)
        };

        foreach (var expectedDeliveryPeriod in expectedDeliveryPeriods)
        {
            actualInstallments.Should().Contain(x => x.AcademicYear == expectedDeliveryPeriod.academicYear && x.DeliveryPeriod == expectedDeliveryPeriod.deliveryPeriod);
        }
    }
}