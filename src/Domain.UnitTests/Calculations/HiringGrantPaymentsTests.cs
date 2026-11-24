using AutoFixture;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Calculations;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Services;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.UnitTests.TestHelpers;
using SFA.DAS.Funding.ApprenticeshipEarnings.TestHelpers;
using SFA.DAS.Funding.ApprenticeshipEarnings.Types;
using System;
using System.Linq;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.Domain.UnitTests.Calculations;

[TestFixture]
public class HiringGrantPaymentsTests
{
    private static readonly DateTime StartDate = new(2026, 10, 1);
    private static readonly DateTime EligibleFrom = new(2026, 10, 1);

    [Test]
    public void Then_Two_Grants_Are_Due_On_Day_90_And_Day_365()
    {
        var result = HiringGrantPayments.GenerateHiringGrantPayments(EmployerType.NonLevy, 18, StartDate, EligibleFrom);

        result.Should().HaveCount(2);
        result.Select(x => x.DueDate).Should().Equal(StartDate.AddDays(89), StartDate.AddDays(364));
        result.Should().OnlyContain(x => x.Amount == 1000m && x.IncentiveType == InstalmentTypes.EmployerHiringGrant);
    }

    [Test]
    public void Then_The_Academic_Year_And_Delivery_Period_Follow_The_Due_Date()
    {
        var result = HiringGrantPayments.GenerateHiringGrantPayments(EmployerType.NonLevy, 18, StartDate, EligibleFrom);

        result[0].AcademicYear.Should().Be(2627);
        result[0].DeliveryPeriod.Should().Be(5);
        result[1].AcademicYear.Should().Be(2728);
        result[1].DeliveryPeriod.Should().Be(2);
    }

    [TestCase(15, false)]
    [TestCase(16, true)]
    [TestCase(24, true)]
    [TestCase(25, false)]
    public void Then_Only_Learners_Aged_16_To_24_Are_Eligible(int age, bool eligible)
    {
        HiringGrantPayments.GenerateHiringGrantPayments(EmployerType.NonLevy, age, StartDate, EligibleFrom).Any().Should().Be(eligible);
    }

    [Test]
    public void Then_Levy_Employers_Are_Not_Eligible()
    {
        HiringGrantPayments.GenerateHiringGrantPayments(EmployerType.Levy, 18, StartDate, EligibleFrom).Should().BeEmpty();
    }

    [TestCase("2026-09-30", false)]
    [TestCase("2026-10-01", true)]
    [TestCase("2027-01-01", true)]
    public void Then_Only_Starts_On_Or_After_The_Configured_Date_Are_Eligible(string startDate, bool eligible)
    {
        HiringGrantPayments.GenerateHiringGrantPayments(EmployerType.NonLevy, 18, DateTime.Parse(startDate), EligibleFrom).Any().Should().Be(eligible);
    }

    [Test]
    public void Then_The_Eligible_From_Date_Is_Whatever_Is_Passed_In()
    {
        var laterEligibleFrom = new DateTime(2027, 1, 1);

        HiringGrantPayments.GenerateHiringGrantPayments(EmployerType.NonLevy, 18, new DateTime(2026, 12, 31), laterEligibleFrom).Should().BeEmpty();
        HiringGrantPayments.GenerateHiringGrantPayments(EmployerType.NonLevy, 18, laterEligibleFrom, laterEligibleFrom).Should().HaveCount(2);
    }
}
