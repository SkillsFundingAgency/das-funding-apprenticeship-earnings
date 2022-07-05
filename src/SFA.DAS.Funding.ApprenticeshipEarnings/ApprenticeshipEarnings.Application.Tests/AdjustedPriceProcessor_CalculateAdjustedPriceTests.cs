using FluentAssertions;
using NUnit.Framework;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.Application.Tests;

public class AdjustedPriceProcessor_CalculateAdjustedPriceTests
{
    private AdjustedPriceProcessor _sut;

    [TestCase(15000, 12000)]
    [TestCase(18777, 15021.60)]
    public void ShouldReturn80PercentOfAgreedPrice(decimal agreedPrice, decimal expectedAdjustedPrice)
    {
        _sut = new AdjustedPriceProcessor();

        var actualAdjustedPrice = _sut.CalculateAdjustedPrice(agreedPrice);

        actualAdjustedPrice.Should().Be(expectedAdjustedPrice);
    }
}