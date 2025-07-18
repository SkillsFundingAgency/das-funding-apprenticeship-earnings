using System;
using FluentAssertions;
using NUnit.Framework;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Apprenticeship;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Calculations;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Extensions;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.Domain.UnitTests.Calculations;

[TestFixture]
public class CompletionInstalmentsTests
{
    [Test]
    public void GenerationCompletionInstalment_ShouldReturnCorrectInstalment()
    {
        // Arrange
        var completionDate = new DateTime(2024, 8, 15); // Period 1
        var completionAmount = 1200m;
        var priceKey = Guid.NewGuid();

        var expectedPeriod = completionDate.ToDeliveryPeriod();
        var expectedYear = completionDate.ToAcademicYear();

        // Act
        var result = CompletionInstalments.GenerationCompletionInstalment(completionDate, completionAmount, priceKey);

        // Assert
        result.Should().NotBeNull();
        result.AcademicYear.Should().Be(expectedYear);
        result.DeliveryPeriod.Should().Be(expectedPeriod);
        result.Amount.Should().Be(completionAmount);
        result.EpisodePriceKey.Should().Be(priceKey);
        result.Type.Should().Be(InstalmentType.Completion);
    }
}