using AutoFixture;
using FluentAssertions;
using NUnit.Framework;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Apprenticeship;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.ApprenticeshipFunding;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Extensions;
using System;
using System.Collections.Generic;
using IncentivePaymentsCalculator = SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Calculations.IncentivePayments;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.Domain.UnitTests.IncentivePayments
{
    [TestFixture]
    public class IncentivePaymentsTests
    {
        private Fixture _fixture;

        [SetUp]
        public void SetUp()
        {
            _fixture = new Fixture();
        }

        [Test]
        public void Should_Return_No_IncentivePayments_When_Outside_Age_Range()
        {
            // Arrange
            var apprenticeshipStartDate = _fixture.Create<DateTime>();
            var apprenticeshipEndDate = apprenticeshipStartDate.AddYears(1);

            // Act
            var result = IncentivePaymentsCalculator.GenerateUnder19sIncentivePayments(19, apprenticeshipStartDate, apprenticeshipEndDate, []);

            // Assert
            result.Should().BeEmpty();
        }

        [Test]
        public void Should_Return_90_Day_Incentives_For_Each_Party_When_Less_Than_365_Apprenticeship()
        {
            // Arrange
            var ageAtStartOfLearning = 17;
            var apprenticeshipStartDate = _fixture.Create<DateTime>();
            var apprenticeshipEndDate = apprenticeshipStartDate.AddMonths(6);

            var expectedIncentiveDate = apprenticeshipStartDate.AddDays(89);

            // Act
            var result = IncentivePaymentsCalculator.GenerateUnder19sIncentivePayments(ageAtStartOfLearning, apprenticeshipStartDate, apprenticeshipEndDate, []);

            // Assert
            result.Should().HaveCount(2);
            var expectedPayments = new[]
            {
                ExpectedIncentivePayment(expectedIncentiveDate, "ProviderIncentive"),
                ExpectedIncentivePayment(expectedIncentiveDate, "EmployerIncentive")
            };
            result.Should().BeEquivalentTo(expectedPayments);
        }

        [Test]
        public void Should_Return_90_Day_and_365_Day_Incentives_For_Each_Party_When_Apprenticeship_Longer_Than_A_Year()
        {
            // Arrange
            var ageAtStartOfLearning = 17;
            var apprenticeshipStartDate = _fixture.Create<DateTime>();
            var apprenticeshipEndDate = apprenticeshipStartDate.AddYears(1);

            var firstIncentiveDate = apprenticeshipStartDate.AddDays(89);
            var secondIncentiveDate = apprenticeshipStartDate.AddDays(364);

            // Act
            var result = IncentivePaymentsCalculator.GenerateUnder19sIncentivePayments(ageAtStartOfLearning, apprenticeshipStartDate, apprenticeshipEndDate, []);

            // Assert
            result.Should().HaveCount(4);
            var expectedPayments = new[]
            {
                ExpectedIncentivePayment(firstIncentiveDate, "ProviderIncentive"),
                ExpectedIncentivePayment(firstIncentiveDate, "EmployerIncentive"),
                ExpectedIncentivePayment(secondIncentiveDate, "ProviderIncentive"),
                ExpectedIncentivePayment(secondIncentiveDate, "EmployerIncentive")
            };
            result.Should().BeEquivalentTo(expectedPayments);
        }

        [Test]
        public void Should_Return_90_Day_and_365_Day_Incentives_For_Each_Party_When_Apprenticeship_Exactly_One_Year()
        {
            // Arrange
            //Given
            var ageAtStartOfLearning = 17;
            var apprenticeshipStartDate = new DateTime(2024, 8, 1);
            var apprenticeshipEndDate = new DateTime(2025, 7, 31);

            //Then
            var firstIncentiveDate = new DateTime(2024, 10, 29);
            var secondIncentiveDate = new DateTime(2025, 7, 31);

            // Act
            var result = IncentivePaymentsCalculator.GenerateUnder19sIncentivePayments(ageAtStartOfLearning, apprenticeshipStartDate, apprenticeshipEndDate, []);

            // Assert
            result.Should().HaveCount(4);
            var expectedPayments = new[]
            {
                ExpectedIncentivePayment(firstIncentiveDate, "ProviderIncentive"),
                ExpectedIncentivePayment(firstIncentiveDate, "EmployerIncentive"),
                ExpectedIncentivePayment(secondIncentiveDate, "ProviderIncentive"),
                ExpectedIncentivePayment(secondIncentiveDate, "EmployerIncentive")
            };
            result.Should().BeEquivalentTo(expectedPayments);
        }

        [Test]
        public void Should_Return_No_Incentives_When_Less_Than_90_Day_Apprenticeship()
        {
            // Arrange
            var ageAtStartOfLearning = 17;
            var apprenticeshipStartDate = _fixture.Create<DateTime>();
            var apprenticeshipEndDate = apprenticeshipStartDate.AddMonths(2);

            // Act
            var result = IncentivePaymentsCalculator.GenerateUnder19sIncentivePayments(ageAtStartOfLearning, apprenticeshipStartDate, apprenticeshipEndDate, []);

            // Assert
            result.Should().BeEmpty();
        }

        [Test]
        public void Should_Adjust_Incentives_Dates_To_Account_For_Break_In_Learning_For_Under19s()
        {
            // Arrange
            var startDate = _fixture.Create<DateTime>();
            var endDate = startDate.AddYears(2);

            var breakInLearning = new EpisodeBreakInLearning(Guid.Empty, startDate.AddDays(30), startDate.AddDays(70), endDate);
            
            // Act
            var result = IncentivePaymentsCalculator.GenerateUnder19sIncentivePayments(17, startDate, endDate, [breakInLearning]);

            // Assert
            var expected90DayIncentiveDate = startDate.AddDays(89 + breakInLearning.Duration);
            var expected365DayIncentiveDate = startDate.AddDays(364 + breakInLearning.Duration);

            var expectedPayments = new[]
            {
                ExpectedIncentivePayment(expected90DayIncentiveDate, "ProviderIncentive"),
                ExpectedIncentivePayment(expected90DayIncentiveDate, "EmployerIncentive"),
                ExpectedIncentivePayment(expected365DayIncentiveDate, "ProviderIncentive"),
                ExpectedIncentivePayment(expected365DayIncentiveDate, "EmployerIncentive")
            };

            result.Should().HaveCount(4);
            result.Should().BeEquivalentTo(expectedPayments);
        }

        [Test]
        public void Should_Adjust_Incentives_Dates_To_Account_For_Multiple_Breaks_In_Learning_For_Under19s()
        {
            // Arrange
            var startDate = _fixture.Create<DateTime>();
            var endDate = startDate.AddYears(2);

            TestContext.WriteLine($"Start Date: {startDate}");

            var break1 = new EpisodeBreakInLearning(Guid.Empty, startDate.AddDays(30), startDate.AddDays(70), endDate); // duration 41
            var break2 = new EpisodeBreakInLearning(Guid.Empty, startDate.AddDays(120), startDate.AddDays(150), endDate); // duration 31

            var breaks = new List<EpisodeBreakInLearning> { break1, break2 };

            // Act
            var result = IncentivePaymentsCalculator.GenerateUnder19sIncentivePayments(17, startDate, endDate, breaks);

            // Assert

            // Expected: 90-day incentive pushed by break1 (41 days) - day 130,
            // but that lands 11 days into break2, so it should be pushed to day 161 (day 150 being end of break 2)
            var expected90DayIncentiveDate = startDate.AddDays(161);

            // Expected: 365-day incentive pushed by both breaks (41 + 31 = 72 days) - day 436,
            var expected365DayIncentiveDate = startDate.AddDays(436);

            var expectedPayments = new[]
            {
                ExpectedIncentivePayment(expected90DayIncentiveDate, "ProviderIncentive"),
                ExpectedIncentivePayment(expected90DayIncentiveDate, "EmployerIncentive"),
                ExpectedIncentivePayment(expected365DayIncentiveDate, "ProviderIncentive"),
                ExpectedIncentivePayment(expected365DayIncentiveDate, "EmployerIncentive")
            };

            result.Should().HaveCount(4);
            result.Should().BeEquivalentTo(expectedPayments);
        }

        [Test]
        public void Should_Not_Generate_Incentives_When_Breaks_Push_90Day_Incentive_Beyond_EndDate_For_Under19s()
        {
            // Arrange
            var ageAtStartOfLearning = 17;
            var startDate = _fixture.Create<DateTime>();
            var endDate = startDate.AddDays(100); // long enough for 90-day incentive normally

            // Break runs from day 40 to day 90 (duration 51 days)
            // This pushes the 90-day incentive to day 140, which is after endDate (day 100).
            var breakInLearning = new EpisodeBreakInLearning(Guid.Empty, startDate.AddDays(40), startDate.AddDays(90), endDate);

            // Act
            var result = IncentivePaymentsCalculator.GenerateUnder19sIncentivePayments(
                ageAtStartOfLearning, startDate, endDate, new List<EpisodeBreakInLearning> { breakInLearning });

            // Assert
            result.Should().BeEmpty("because the break in learning pushes the 90-day incentive date beyond the apprenticeship end date");
        }

        [Test]
        public void Should_Adjust_Incentives_Dates_To_Account_For_Break_In_Learning_For_19To24()
        {
            // Arrange
            var startDate = _fixture.Create<DateTime>();
            var endDate = startDate.AddYears(2);

            var breakInLearning = new EpisodeBreakInLearning(Guid.Empty, startDate.AddDays(30), startDate.AddDays(70), endDate);

            // Act
            var result = IncentivePaymentsCalculator.Generate19To24IncentivePayments(
                20, startDate, endDate, hasEHCP: true, isCareLeaver: false, careLeaverEmployerConsentGiven: false,
                new List<EpisodeBreakInLearning> { breakInLearning });

            // Assert
            var expected90DayIncentiveDate = startDate.AddDays(89 + breakInLearning.Duration);
            var expected365DayIncentiveDate = startDate.AddDays(364 + breakInLearning.Duration);

            var expectedPayments = new[]
            {
                ExpectedIncentivePayment(expected90DayIncentiveDate, "ProviderIncentive"),
                ExpectedIncentivePayment(expected90DayIncentiveDate, "EmployerIncentive"),
                ExpectedIncentivePayment(expected365DayIncentiveDate, "ProviderIncentive"),
                ExpectedIncentivePayment(expected365DayIncentiveDate, "EmployerIncentive")
            };

            result.Should().HaveCount(4);
            result.Should().BeEquivalentTo(expectedPayments, opts => opts.Excluding(p => p.Amount));
        }

        [Test]
        public void Should_Adjust_Incentives_Dates_To_Account_For_Multiple_Breaks_In_Learning_For_19To24()
        {
            // Arrange
            var startDate = _fixture.Create<DateTime>();
            var endDate = startDate.AddYears(2);

            var break1 = new EpisodeBreakInLearning(Guid.Empty, startDate.AddDays(30), startDate.AddDays(70), endDate); // duration 41
            var break2 = new EpisodeBreakInLearning(Guid.Empty, startDate.AddDays(120), startDate.AddDays(150), endDate); // duration 31

            var breaks = new List<EpisodeBreakInLearning> { break1, break2 };

            // Act
            var result = IncentivePaymentsCalculator.Generate19To24IncentivePayments(
                20, startDate, endDate, hasEHCP: true, isCareLeaver: false, careLeaverEmployerConsentGiven: false, breaks);

            // Assert
            var expected90DayIncentiveDate = startDate.AddDays(161); // pushed into break2, then carried forward
            var expected365DayIncentiveDate = startDate.AddDays(436); // 364 + 41 + 31

            var expectedPayments = new[]
            {
                ExpectedIncentivePayment(expected90DayIncentiveDate, "ProviderIncentive"),
                ExpectedIncentivePayment(expected90DayIncentiveDate, "EmployerIncentive"),
                ExpectedIncentivePayment(expected365DayIncentiveDate, "ProviderIncentive"),
                ExpectedIncentivePayment(expected365DayIncentiveDate, "EmployerIncentive")
            };

            result.Should().HaveCount(4);
            result.Should().BeEquivalentTo(expectedPayments, opts => opts.Excluding(p => p.Amount));
        }

        [Test]
        public void Should_Not_Generate_Incentives_When_Breaks_Push_90Day_Incentive_Beyond_EndDate_For_19To24()
        {
            // Arrange
            var startDate = _fixture.Create<DateTime>();
            var endDate = startDate.AddDays(100);

            var breakInLearning = new EpisodeBreakInLearning(Guid.Empty, startDate.AddDays(40), startDate.AddDays(90), endDate);

            // Act
            var result = IncentivePaymentsCalculator.Generate19To24IncentivePayments(
                20, startDate, endDate, hasEHCP: true, isCareLeaver: false, careLeaverEmployerConsentGiven: false,
                new List<EpisodeBreakInLearning> { breakInLearning });

            // Assert
            result.Should().BeEmpty("because the break pushes the 90-day incentive beyond the apprenticeship end date");
        }

        private static IncentivePayment ExpectedIncentivePayment(DateTime dueDate, string incentiveType)
        {
            return new IncentivePayment
            {
                AcademicYear = dueDate.ToAcademicYear(),
                DueDate = dueDate,
                DeliveryPeriod = dueDate.ToDeliveryPeriod(),
                IncentiveType = incentiveType,
                Amount = 500
            };
        }
    }
}
