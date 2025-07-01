using System;
using AutoFixture;
using FluentAssertions;
using NUnit.Framework;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.ApprenticeshipFunding;
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
            var result = IncentivePaymentsCalculator.GenerateUnder19sIncentivePayments(19, apprenticeshipStartDate, apprenticeshipEndDate);

            // Assert
            result.Should().BeEmpty();
        }

        [Test]
        public void Should_Return_90_Day_Incentives_For_Each_Party_When_Less_Than_365_Apprenticeship()
        {
            // Arrange
            var AgeAtStartOfLearning = 17;
            var apprenticeshipStartDate = _fixture.Create<DateTime>();
            var apprenticeshipEndDate = apprenticeshipStartDate.AddMonths(6);

            var expectedIncentiveDate = apprenticeshipStartDate.AddDays(89);

            // Act
            var result = IncentivePaymentsCalculator.GenerateUnder19sIncentivePayments(AgeAtStartOfLearning, apprenticeshipStartDate, apprenticeshipEndDate);

            // Assert
            result.Should().HaveCount(2);
            result.Should().ContainEquivalentOf(new IncentivePayment
            {
                AcademicYear = expectedIncentiveDate.ToAcademicYear(),
                DueDate = expectedIncentiveDate,
                Amount = 500,
                DeliveryPeriod = expectedIncentiveDate.ToDeliveryPeriod(),
                IncentiveType = "ProviderIncentive"
            });
            result.Should().ContainEquivalentOf(new IncentivePayment
            {
                AcademicYear = expectedIncentiveDate.ToAcademicYear(),
                DueDate = expectedIncentiveDate,
                Amount = 500,
                DeliveryPeriod = expectedIncentiveDate.ToDeliveryPeriod(),
                IncentiveType = "EmployerIncentive"
            });
        }

        [Test]
        public void Should_Return_90_Day_and_365_Day_Incentives_For_Each_Party_When_Apprenticeship_Longer_Than_A_Year()
        {
            // Arrange
            var AgeAtStartOfLearning = 17;
            var apprenticeshipStartDate = _fixture.Create<DateTime>();
            var apprenticeshipEndDate = apprenticeshipStartDate.AddYears(1);

            var firstIncentiveDate = apprenticeshipStartDate.AddDays(89);
            var secondIncentiveDate = apprenticeshipStartDate.AddDays(364);

            // Act
            var result = IncentivePaymentsCalculator.GenerateUnder19sIncentivePayments(AgeAtStartOfLearning, apprenticeshipStartDate, apprenticeshipEndDate);

            // Assert
            result.Should().HaveCount(4);
            result.Should().ContainEquivalentOf(new IncentivePayment
            {
                AcademicYear = firstIncentiveDate.ToAcademicYear(),
                DueDate = firstIncentiveDate,
                Amount = 500,
                DeliveryPeriod = firstIncentiveDate.ToDeliveryPeriod(),
                IncentiveType = "ProviderIncentive"
            });
            result.Should().ContainEquivalentOf(new IncentivePayment
            {
                AcademicYear = firstIncentiveDate.ToAcademicYear(),
                DueDate = firstIncentiveDate,
                Amount = 500,
                DeliveryPeriod = firstIncentiveDate.ToDeliveryPeriod(),
                IncentiveType = "EmployerIncentive"
            });
            result.Should().ContainEquivalentOf(new IncentivePayment
            {
                AcademicYear = secondIncentiveDate.ToAcademicYear(),
                DueDate = secondIncentiveDate,
                Amount = 500,
                DeliveryPeriod = secondIncentiveDate.ToDeliveryPeriod(),
                IncentiveType = "ProviderIncentive"
            });
            result.Should().ContainEquivalentOf(new IncentivePayment
            {
                AcademicYear = secondIncentiveDate.ToAcademicYear(),
                DueDate = secondIncentiveDate,
                Amount = 500,
                DeliveryPeriod = secondIncentiveDate.ToDeliveryPeriod(),
                IncentiveType = "EmployerIncentive"
            });
        }

        [Test]
        public void Should_Return_90_Day_and_365_Day_Incentives_For_Each_Party_When_Apprenticeship_Exactly_One_Year()
        {
            // Arrange
            //Given
            var AgeAtStartOfLearning = 17;
            var apprenticeshipStartDate = new DateTime(2024, 8, 1);
            var apprenticeshipEndDate = new DateTime(2025, 7, 31);

            //Then
            var firstIncentiveDate = new DateTime(2024, 10, 29);
            var secondIncentiveDate = new DateTime(2025, 7, 31);

            // Act
            var result = IncentivePaymentsCalculator.GenerateUnder19sIncentivePayments(AgeAtStartOfLearning, apprenticeshipStartDate, apprenticeshipEndDate);

            // Assert
            result.Should().HaveCount(4);
            result.Should().ContainEquivalentOf(new IncentivePayment
            {
                AcademicYear = firstIncentiveDate.ToAcademicYear(),
                DueDate = firstIncentiveDate,
                Amount = 500,
                DeliveryPeriod = firstIncentiveDate.ToDeliveryPeriod(),
                IncentiveType = "ProviderIncentive"
            });
            result.Should().ContainEquivalentOf(new IncentivePayment
            {
                AcademicYear = firstIncentiveDate.ToAcademicYear(),
                DueDate = firstIncentiveDate,
                Amount = 500,
                DeliveryPeriod = firstIncentiveDate.ToDeliveryPeriod(),
                IncentiveType = "EmployerIncentive"
            });
            result.Should().ContainEquivalentOf(new IncentivePayment
            {
                AcademicYear = secondIncentiveDate.ToAcademicYear(),
                DueDate = secondIncentiveDate,
                Amount = 500,
                DeliveryPeriod = secondIncentiveDate.ToDeliveryPeriod(),
                IncentiveType = "ProviderIncentive"
            });
            result.Should().ContainEquivalentOf(new IncentivePayment
            {
                AcademicYear = secondIncentiveDate.ToAcademicYear(),
                DueDate = secondIncentiveDate,
                Amount = 500,
                DeliveryPeriod = secondIncentiveDate.ToDeliveryPeriod(),
                IncentiveType = "EmployerIncentive"
            });
        }

        [Test]
        public void Should_Return_No_Incentives_When_Less_Than_90_Day_Apprenticeship()
        {
            // Arrange
            var AgeAtStartOfLearning = 17;
            var apprenticeshipStartDate = _fixture.Create<DateTime>();
            var apprenticeshipEndDate = apprenticeshipStartDate.AddMonths(2);

            var expectedIncentiveDate = apprenticeshipStartDate.AddDays(90);

            // Act
            var result = IncentivePaymentsCalculator.GenerateUnder19sIncentivePayments(AgeAtStartOfLearning, apprenticeshipStartDate, apprenticeshipEndDate);

            // Assert
            result.Should().BeEmpty();
        }
    }
}
