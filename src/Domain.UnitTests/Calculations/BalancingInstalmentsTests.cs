using System;
using System.Collections.Generic;
using FluentAssertions;
using NUnit.Framework;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Apprenticeship;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Calculations;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Extensions;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.Domain.UnitTests.Calculations;

[TestFixture]
public class BalancingInstalmentsTests
{
    [Test]
    public void BalanceInstalmentsForCompletion_ShouldReturnOriginalList_WhenNoInstalmentInCompletionPeriod()
    {
        // Arrange
        var completionDate = new DateTime(2024, 8, 1); //R01
        var completionAmount = 1000m;
        var priceKey = Guid.NewGuid();
        var instalments = new List<Instalment>
        {
            new Instalment(2324, 12, 500, priceKey),
            new Instalment(2324, 11, 300, priceKey)
        };

        // Act
        var result = BalancingInstalments.BalanceInstalmentsForCompletion(completionDate, completionAmount, new List<Instalment>(instalments));

        // Assert
        result.Should().BeEquivalentTo(instalments);
    }

    [Test]
    public void BalanceInstalmentsForCompletion_ShouldRemoveInstalmentsPastCompletion_AndAddBalancingInstalment()
    {
        // Arrange
        var completionDate = new DateTime(2024, 6, 1); //R11
        var completionPeriod = completionDate.ToDeliveryPeriod();
        var completionYear = completionDate.ToAcademicYear();
        var priceKey = Guid.NewGuid();

        var instalments = new List<Instalment>
        {
            new Instalment(completionYear, 9, 200, priceKey),
            new Instalment(completionYear, 10, 300, priceKey),
            new Instalment(completionYear, 11, 100, priceKey),
            new Instalment(completionYear, 12, 400, priceKey)
        };

        var expectedBalancingAmount = 500;

        // Act
        var result = BalancingInstalments.BalanceInstalmentsForCompletion(completionDate, 1000m, new List<Instalment>(instalments));

        // Assert
        result.Should().ContainSingle(x =>
            x.Type == InstalmentType.Balancing &&
            x.AcademicYear == completionYear &&
            x.DeliveryPeriod == completionPeriod &&
            x.Amount == expectedBalancingAmount);

        result.Should().NotContain(x => x.DeliveryPeriod > completionPeriod);
        result.Should().NotContain(x => x.DeliveryPeriod == completionPeriod && x.Type != InstalmentType.Balancing);
    }

    [Test]
    public void BalanceInstalmentsForCompletion_ShouldNotAddBalancingInstalment_IfCompletionAfterLastInstalment()
    {
        // Arrange
        var completionDate = new DateTime(2024, 7, 1);
        var completionYear = completionDate.ToAcademicYear();
        var completionPeriod = completionDate.ToDeliveryPeriod();
        var priceKey = Guid.NewGuid();

        var instalments = new List<Instalment>
        {
            new Instalment(completionYear, 11, 100, priceKey),
            new Instalment(completionYear, 10, 200, priceKey)
        };

        // Act
        var result = BalancingInstalments.BalanceInstalmentsForCompletion(completionDate, 500m, new List<Instalment>(instalments));

        // Assert
        result.Should().NotContain(x => x.Type == InstalmentType.Balancing);
        result.Should().NotContain(x => x.DeliveryPeriod == completionPeriod);
    }
}