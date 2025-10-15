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
        var priceKey = Guid.NewGuid();
        var instalments = new List<Instalment>
        {
            new Instalment(2324, 12, 500, priceKey),
            new Instalment(2324, 11, 300, priceKey)
        };
        var plannedEndDate = new DateTime(2024, 7, 15);

        // Act
        var result = BalancingInstalments.BalanceInstalmentsForCompletion(completionDate, new List<Instalment>(instalments), plannedEndDate);

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
        var plannedEndDate = new DateTime(2024, 7, 15);

        var expectedBalancingAmount = 500;

        // Act
        var result = BalancingInstalments.BalanceInstalmentsForCompletion(completionDate, new List<Instalment>(instalments), plannedEndDate);

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
        var plannedEndDate = new DateTime(2024, 7, 11);

        // Act
        var result = BalancingInstalments.BalanceInstalmentsForCompletion(completionDate, new List<Instalment>(instalments), plannedEndDate);

        // Assert
        result.Should().NotContain(x => x.Type == InstalmentType.Balancing);
        result.Should().NotContain(x => x.DeliveryPeriod == completionPeriod);
    }

    /// <summary>
    /// This test covers a specific scenario in FLP-1366 where the planned end date is the census date for the month so an instalment exists for that month;
    /// but the learner completes early in the same month so that month's payment needs to be balancing not regular.
    /// </summary>
    [Test]
    public void BalanceInstalmentsForCompletion_LastInstalmentShouldBeBalancing_IfCompletionEarlyButInSameMonthAsPlannedEndDate()
    {
        // Arrange
        var completionDate = new DateTime(2024, 6, 3); //early in R11
        var completionYear = completionDate.ToAcademicYear();
        var completionPeriod = completionDate.ToDeliveryPeriod();
        var priceKey = Guid.NewGuid();

        var instalments = new List<Instalment>
        {
            new Instalment(completionYear, 11, 100, priceKey), //existing regular instalment for R11
            new Instalment(completionYear, 10, 200, priceKey)
        };
        var plannedEndDate = new DateTime(2024, 6, 30); //census date for R11

        // Act
        var result = BalancingInstalments.BalanceInstalmentsForCompletion(completionDate, new List<Instalment>(instalments), plannedEndDate);

        // Assert
        result.Should().Contain(x => x.DeliveryPeriod == completionPeriod && x.Type == InstalmentType.Balancing);
    }
}