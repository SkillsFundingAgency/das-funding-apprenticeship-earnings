using FluentAssertions;
using NUnit.Framework;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.Domain.UnitTests;

public class OnProgramTotalPriceCalculator_CalculateAdjustedPriceTests
{
    private OnProgramTotalPriceCalculator _sut;

    [TestCase(15000, 12000)]
    [TestCase(18777, 15021.60)]
    public void ShouldReturn80PercentOfAgreedPrice(decimal agreedPrice, decimal expectedAdjustedPrice)
    {
        _sut = new OnProgramTotalPriceCalculator();

        var actualAdjustedPrice = _sut.CalculateOnProgramTotalPrice(agreedPrice);

        actualAdjustedPrice.Should().Be(expectedAdjustedPrice);
    }
}