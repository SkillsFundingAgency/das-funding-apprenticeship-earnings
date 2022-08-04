using FluentAssertions;
using NUnit.Framework;
using SFA.DAS.Apprenticeships.Events;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.Domain.UnitTests;

public class AdjustedPriceCalculator_CalculateAdjustedPriceTests
{
    private AdjustedPriceCalculator _sut;

    [Test]
    public void ShouldCapPriceByFundingBandMaximumWhenLowerThanAgreedPrice()
    {
        _sut = new AdjustedPriceCalculator();

        var result = _sut.CalculateAdjustedPrice(new ApprenticeshipCreatedEvent { AgreedPrice = 20000, FundingBandMaximum = 16000 });

        result.Should().Be(16000);
    }

    [Test]
    public void ShouldNotCapPriceByFundingBandMaximumWhenHigherThanAgreedPrice()
    {
        _sut = new AdjustedPriceCalculator();

        var result = _sut.CalculateAdjustedPrice(new ApprenticeshipCreatedEvent { AgreedPrice = 20000, FundingBandMaximum = 27000 });

        result.Should().Be(20000);
    }
}