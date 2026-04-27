using System;
using System.Collections.Generic;
using FluentAssertions;
using NUnit.Framework;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Calculations;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Models;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Models.Apprenticeship;
using SFA.DAS.Funding.ApprenticeshipEarnings.Types;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.Domain.UnitTests.Calculations;

[TestFixture]
public class OnProgramPaymentsTests
{
    [Test]
    public void RemoveAfterLastDayOfLearning_ShouldKeepCompletionInstalments_EvenIfAfterLastDayOfLearning()
    {
        // Arrange
        var lastDayOfLearning = new DateTime(2022, 2, 15); // Period 7
        var priceKey = Guid.NewGuid();
        var prices = new List<ApprenticeshipPrice> { new ApprenticeshipPrice(priceKey, new DateTime(2021, 8, 1), new DateTime(2022, 7, 31), 15000) };
        
        var instalments = new List<ApprenticeshipInstalment>
        {
            new ApprenticeshipInstalment(2122, 6, 1000, priceKey), // Before
            new ApprenticeshipInstalment(2122, 7, 6000, priceKey, InstalmentType.Balancing), // At
            new ApprenticeshipInstalment(2122, 8, 3000, priceKey, InstalmentType.Completion), // After
            new ApprenticeshipInstalment(2122, 9, 1000, priceKey) // After (Regular)
        };

        // Act
        var result = OnProgramPayments.RemoveAfterLastDayOfLearning(new List<ApprenticeshipInstalment>(instalments), prices, lastDayOfLearning);

        // Assert
        result.Should().Contain(x => x.Type == InstalmentType.Completion && x.DeliveryPeriod == 8);
        result.Should().Contain(x => x.Type == InstalmentType.Balancing && x.DeliveryPeriod == 7);
        result.Should().Contain(x => x.DeliveryPeriod == 6);
        result.Should().NotContain(x => x.DeliveryPeriod == 9);
    }
}
