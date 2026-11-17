using FluentAssertions;
using NUnit.Framework;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Calculations;
using System;
using System.Linq;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.Domain.UnitTests.Calculations;

[TestFixture]
public class LearningSupportPaymentsWithoutEarningsTests
{
    [Test]
    public void GenerateLearningSupportPaymentsWithoutEarnings_ShouldReturnEmptyList_WhenStartDateIsAfterEndDate()
    {
        var result = LearningSupportPayments.GenerateLearningSupportPaymentsWithoutEarnings(new DateTime(2023, 12, 1), new DateTime(2023, 11, 30));

        result.Should().BeEmpty();
    }

    [Test]
    public void GenerateLearningSupportPaymentsWithoutEarnings_ShouldGenerateTheSameScheduleWithZeroAmounts()
    {
        var startDate = new DateTime(2023, 10, 1);
        var endDate = new DateTime(2023, 12, 31);

        var calculated = LearningSupportPayments.GenerateLearningSupportPayments(startDate, endDate);
        var withoutEarnings = LearningSupportPayments.GenerateLearningSupportPaymentsWithoutEarnings(startDate, endDate);

        withoutEarnings.Should().HaveCount(3);
        withoutEarnings.Should().OnlyContain(x => x.Amount == 0m);
        withoutEarnings
            .Select(x => (x.AcademicYear, x.DeliveryPeriod, x.DueDate, x.AdditionalPaymentType))
            .Should().Equal(calculated.Select(x => (x.AcademicYear, x.DeliveryPeriod, x.DueDate, x.AdditionalPaymentType)));
    }

    [Test]
    public void GenerateLearningSupportPayments_ShouldStillUseTheStandardAmount()
    {
        var result = LearningSupportPayments.GenerateLearningSupportPayments(new DateTime(2023, 11, 1), new DateTime(2023, 11, 30));

        result.Should().ContainSingle().Which.Amount.Should().Be(150);
    }
}
