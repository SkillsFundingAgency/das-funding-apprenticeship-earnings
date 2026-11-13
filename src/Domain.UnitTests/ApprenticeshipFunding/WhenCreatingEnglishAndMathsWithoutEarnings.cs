using FluentAssertions;
using NUnit.Framework;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Calculations;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Interfaces;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Models.EnglishAndMaths;
using SFA.DAS.Funding.ApprenticeshipEarnings.TestHelpers;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.Domain.UnitTests.ApprenticeshipFunding;

[TestFixture]
public class WhenCreatingEnglishAndMathsWithoutEarnings
{
    private readonly DateTime _startDate = new(2024, 1, 1);
    private readonly DateTime _endDate = new(2024, 12, 31);
    private List<IPeriodInLearning> _periods = null!;
    private List<EnglishAndMathsAdditionalPayment> _learningSupport = null!;

    [SetUp]
    public void SetUp()
    {
        _periods = [PeriodInLearningHelper.Create(_startDate, _endDate, _endDate)];
        _learningSupport = LearningSupportPayments
            .GenerateLearningSupportPaymentsWithoutEarnings(_startDate, _endDate)
            .Select(p => new EnglishAndMathsAdditionalPayment(p.AcademicYear, p.DeliveryPeriod, p.Amount, p.DueDate, p.AdditionalPaymentType))
            .ToList();
    }

    [Test]
    public void Then_No_Instalments_Are_Generated()
    {
        var course = CreateWithoutEarnings();

        course.Instalments.Should().BeEmpty();
        course.GetEntity().Instalments.Should().BeEmpty();
    }

    [Test]
    public void Then_Course_Details_And_Learning_Support_Are_Stored()
    {
        var course = CreateWithoutEarnings();

        course.Course.Should().Be("English Level 2");
        course.LearnAimRef.Should().Be("ENG2");
        course.Amount.Should().Be(1200m);
        course.StartDate.Should().Be(_startDate);
        course.EndDate.Should().Be(_endDate);
        course.PeriodsInLearning.Should().HaveCount(1);
        course.AdditionalPayments.Should().HaveCount(_learningSupport.Count);
        course.AdditionalPayments.Should().OnlyContain(x => x.Amount == 0m);
    }

    [Test]
    public void Then_Calculate_Generates_The_Same_Instalments_As_The_Constructor()
    {
        var uncalculated = CreateWithoutEarnings();
        var expected = new EnglishAndMaths(_startDate, _endDate, "English Level 2", "ENG2", 1200m, null, null, null, null, _periods);

        uncalculated.Calculate();

        uncalculated.Instalments.Should().NotBeEmpty();
        uncalculated.Instalments
            .Select(x => (x.AcademicYear, x.DeliveryPeriod, x.Amount, x.Type))
            .Should().BeEquivalentTo(expected.Instalments.Select(x => (x.AcademicYear, x.DeliveryPeriod, x.Amount, x.Type)));
        uncalculated.GetEntity().Instalments.Should().HaveCount(expected.Instalments.Count);
    }

    [Test]
    public void Then_The_Constructor_Still_Calculates_Instalments()
    {
        var course = new EnglishAndMaths(_startDate, _endDate, "English Level 2", "ENG2", 1200m, null, null, null, null, _periods);

        course.Instalments.Should().NotBeEmpty();
    }

    private EnglishAndMaths CreateWithoutEarnings() =>
        EnglishAndMaths.CreateWithoutEarnings(_startDate, _endDate, "English Level 2", "ENG2", 1200m, null, null, null, null, _periods, _learningSupport);
}
