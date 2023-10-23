using System;
using System.Collections.Generic;
using FluentAssertions;
using NUnit.Framework;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Apprenticeship;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.ApprenticeshipFunding;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.Domain.UnitTests;

public class InstallmentsGenerator_GenerateTests
{
    private InstalmentsGenerator _sut;

    [SetUp]
    public void SetUp()
    {
        _sut = new InstalmentsGenerator();
    }

    [TestCase(12000, 2010, 01, 05, 2012, 01, 04, 24)]
    [TestCase(12000, 2010, 01, 05, 2011, 12, 31, 24)]
    [TestCase(12000, 2010, 01, 05, 2011, 12, 30, 23)]
    public void ShouldGenerateCorrectNumberOfInstallments(decimal total, int startYear, int startMonth, int startDay,
        int endYear, int endMonth, int endDay, int expectedNumberOfInstallments)
    {
        var startDate = new DateTime(startYear, startMonth, startDay);
        var endDate = new DateTime(endYear, endMonth, endDay);

        var actualInstallments = _sut.Generate(total, startDate, endDate);

        actualInstallments.Should().HaveCount(expectedNumberOfInstallments);
    }

    [TestCase(12000, 2010, 01, 05, 2011, 12, 31, 500)]
    [TestCase(12000, 2010, 01, 05, 2011, 12, 30, 521.73913)]
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
            (1819,5)
        };

        foreach (var expectedDeliveryPeriod in expectedDeliveryPeriods)
        {
            actualInstallments.Should().Contain(x => x.AcademicYear == expectedDeliveryPeriod.academicYear && x.DeliveryPeriod == expectedDeliveryPeriod.deliveryPeriod);
        }
    }

    [TestCase("2020-01-05", "2020-10-05", "2022-01-04", 24)]
    [TestCase("2020-01-05", "2020-10-05", "2021-12-31", 24)]
    [TestCase("2020-01-05", "2020-10-05", "2021-12-30", 23)]
    public void ShouldRegenerateCorrectNumberOfInstallments(string startDateString, string priceChangeDateString, string endDateString, int expectedNumberOfInstallments)
    {
        //  Arrange
        var orignalPrice = 12000;
        var updatedPrice = 12000;
        var startDate = DateTime.Parse(startDateString);
        var endDate = DateTime.Parse(endDateString);
        var priceChangeDate = DateTime.Parse(priceChangeDateString);
        var orginalInstalments = _sut.Generate(orignalPrice, startDate, endDate);

        //  Act
        var recalculatedInstallments = _sut.Generate(updatedPrice, priceChangeDate, endDate, orginalInstalments);

        //  Assert
        recalculatedInstallments.Should().HaveCount(expectedNumberOfInstallments);
    }

    [TestCase(12000, 13600, "2023-08-23", "2023-09-23", "2025-4-23", 600, 684.21)]
    [TestCase(12000, 14400, "2023-08-23", "2024-12-01", "2025-4-23", 600, 1200)]
    [TestCase(12000, 6400, "2023-08-23", "2024-08-01", "2025-4-23", 600, -100)]
    public void ShouldRegenerateCorrectMonthlyInstallmentAmount(
        decimal totalBeforeChange,
        decimal totalAfterChange,
        string startDateString, 
        string priceChangeDateString, 
        string endDateString, 
        decimal expectedMonthlyBeforeChange,
        decimal expectedMonthlyAfterChange)
    {
        //  Arrange
        var startDate = DateTime.Parse(startDateString);
        var endDate = DateTime.Parse(endDateString);
        var priceChangeDate = DateTime.Parse(priceChangeDateString);
        var orginalInstalments = _sut.Generate(totalBeforeChange, startDate, endDate);

        //  Act
        var recalculatedInstallments = _sut.Generate(totalAfterChange, priceChangeDate, endDate, orginalInstalments);

        //  Assert
        foreach(var instalment in recalculatedInstallments)
        {
            if(IsBeforePriceChangeDate(instalment, priceChangeDate))
            {
                instalment.Amount.Should().BeApproximately(expectedMonthlyBeforeChange, 0.01m);
            }
            else
            {
                instalment.Amount.Should().BeApproximately(expectedMonthlyAfterChange, 0.01m);
            }

        }

    }

    private bool IsBeforePriceChangeDate(Earning instalment, DateTime priceChangeDate)
    {
        if (instalment.AcademicYear < priceChangeDate.ToAcademicYear())
            return true;


        if (instalment.AcademicYear > priceChangeDate.ToAcademicYear())
            return false;


        if (instalment.DeliveryPeriod < priceChangeDate.ToDeliveryPeriod())
            return true;

        return false;
    }   

}