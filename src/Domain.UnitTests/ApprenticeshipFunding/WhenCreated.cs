using AutoFixture;
using FluentAssertions;
using NUnit.Framework;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.Domain.UnitTests.ApprenticeshipFunding;

[TestFixture]
public class WhenCreated
{
    private Fixture _fixture;

    [SetUp]
    public void SetUp()
    {
        _fixture = new Fixture();
    }

    [Test]
    public void WhenAgreedPriceLessThenFundingBandMaximumThenFundingValuesAreCalculatedCorrectly()
    {
        var agreedPrice = _fixture.Create<decimal>();
        var fundingBandMaximum = agreedPrice + 1;
        var apprenticeshipFunding = new Domain.ApprenticeshipFunding.ApprenticeshipFunding(agreedPrice, fundingBandMaximum);

        apprenticeshipFunding.OnProgramTotal.Should().Be(agreedPrice * 0.8m);
        apprenticeshipFunding.CompletionPayment.Should().Be(agreedPrice * 0.2m);
    }

    [Test]
    public void WhenAgreedPriceMoreThenFundingBandMaximumThenFundingValuesAreCalculatedCorrectly()
    {
        var agreedPrice = _fixture.Create<decimal>();
        var fundingBandMaximum = agreedPrice - 1000;
        var apprenticeshipFunding = new Domain.ApprenticeshipFunding.ApprenticeshipFunding(agreedPrice, fundingBandMaximum);

        apprenticeshipFunding.OnProgramTotal.Should().Be(fundingBandMaximum * 0.8m);
        apprenticeshipFunding.CompletionPayment.Should().Be(fundingBandMaximum * 0.2m);
    }
}