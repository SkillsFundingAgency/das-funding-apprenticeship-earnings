using FluentAssertions;
using NUnit.Framework;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.Domain.UnitTests;

[TestFixture]
public class WhenCalculatingCoInvestment
{
    [Test]
    public void WhenIsNotFullyFundedThenEmployerContributionIs95Percent()
    {
        var totalAmount = 2000;
        var coinvestment = CoInvestment.Calculate(false, totalAmount);

        coinvestment.EmployerContribution.Should().Be(100);
    }

    [Test]
    public void WhenIsNotFullyFundedThenGovernmentContributionIs5Percent()
    {
        var totalAmount = 2000;
        var coinvestment = CoInvestment.Calculate(false, totalAmount);

        coinvestment.GovernmentContribution.Should().Be(1900);
    }

    [Test]
    public void WhenIsFullyFundedThenEmployerContributionIsZero()
    {
        var totalAmount = 2000;
        var coinvestment = CoInvestment.Calculate(true, totalAmount);

        coinvestment.EmployerContribution.Should().Be(0);
    }

    [Test]
    public void WhenIsFullyFundedThenGovernmentContributionIs100Percent()
    {
        var totalAmount = 2000;
        var coinvestment = CoInvestment.Calculate(true, totalAmount);

        coinvestment.GovernmentContribution.Should().Be(totalAmount);
    }
}